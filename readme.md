# Azure IoT Hub
Azure IoT Hub to usługa Microsoftu dedykowana do przetwarzania duzej ilosci danych pomiarowych z urządzeń IoT.
Oparta jest o inną usługę Azure Event Hub lecz dodatkowo wprowadza pewne funkcjonalności przydatne w przypadku urządzeń IoT (przechowywanie konfiguracji, zarządzeniami urządzeniami)

IoT zapewnia: 
- rejestrowanie urządzen i ich autentykacje
- przetwarzanie komunikatów
- zarządzenia urządzeniami

## Warstwy
Usługa Azure IoT Hub jest dostępna w róznych planach taryfowych.
Nalezy zwrocic uwagę na limity oraz szczegolnie na dostępne funkcje.

- **Free** - darmowa, ograniczenie do 8000 komunikatów dziennie, wszystkie dostępne funkcje.
- **Basic** - płatna, brak obsługi Twins oraz Module Edge
- **Standard** - płatna, wszystkie dostępne funkcje.

uwaga: jeśli komunikat przekroczy 4Kb to naliczany jest podwójnie (lub wielokrotnie zaleznie od wielkosci komunikatu).


## Scenariusze komunikacji

Azure IoT Hub obsluguje rozne scenariusze komunikacji:
- Przesyłanie komunikatów z urządzenia do chmury (D2C)
- Przesyłanie komunikatów z chmury do urządzenia (C2D)
- Przesyłanie duzych plików z urządzenia do chmury (Blob)
- Bezpośrednie wywoływanie metod na urządzeniu (Direct)
- Synchronizowanie konfiguracji urządzeń (Twins)
- Przesyłanie danych strumieniowych (Streaming)

## Protokoły komunikacji

Azure Iot Hub obsługuje następujące protokoły komunikacyjne:
- HTTP
- MQTT
- APMQ

## Formaty
Treśc komunikatów (payload) moze byc w dowolnym formacie, ale najcześciej stosowany jest json.

W celu ograniczenia wielkości komunikatów warto zwrócic uwage na formaty:
- ProtoBuf
- MessagePack

## Synchronizacja konfiguracji urządzeń
Przy duzej ilości urządzeń pojawia się problem zarządzania ich konfiguracją i śledzenia ich stanu (poziom naładowania baterii)
Azure Iot Hub wprowadza pojęcie urządzeń bliźniaczych _(Device Twins)_.
Polega to na synchronizacji pliku konfiguracyjnego w formacie json pomiędzy chmurą na urządzeniem.

Konfiguracja podzielona jest na 3 części:
- **Tags** - atrybuty urządzeń przechowywane w Iot Hub (np. region, najemca). Urządzenie nie ma do nich dostepu, lecz ulatwia wyszukiwanie sprzętu po stronie serwera.
  
- **Desired properties** - parametry urządzenia przesyłane z chmury do urządzenia. (np. interwał, progi alarmowe). Urządzenie moze tylko odczytac te wartości, lecz nie moze ich zmieniac. 
  
- **Reported properties** - parametry urządzenia przesyłane z urządzenia do chmury (np. wersja firmware, stan baterii, typ połączenia). IoT Hub moze tylko odczytac te wartości lecz nie moze ich zmieniac.


## Przydatne narzedzia

- Visual Studio Code
- Azure IoT Hub Toolkit
  - Zarządzanie Iot Hub
  - Dodawanie, usuwanie urządzeń
  - Generowanie kodu (C#, HTTP, Java, Python itd)
- **Azure IoT Device Workbench**
  - programowanie na urządzeniach protypowych IoT
  - obsługa MXChip IoT DevKit, ESP32, Raspberry Pi


## Obsługa konfiguracji

Instalacja
~~~ bash
dotnet add package Microsoft.Azure.Devices.Client
~~~

### Desired Properties

azure-iot-device-twin.json

~~~ json

 "properties": {
        "desired": {
            "interval": 3000,
            "tempThreshold": 100
        }

~~~

SimulatedDevice.cs
~~~ csharp
public async Task Init()
{
    Twin twin = await deviceClient.GetTwinAsync();

    // Get desired properties
    await OnDesiredPropertiesUpdate(twin.Properties.Desired, deviceClient);

    // Tracking desired properties changes
    await deviceClient.SetDesiredPropertyUpdateCallbackAsync(OnDesiredPropertiesUpdate, deviceClient);
}

public Task OnDesiredPropertiesUpdate(TwinCollection desiredProperties, object userContext)
{
    if (desiredProperties["interval"] != null)
    {
        int interval = desiredProperties["interval"];
    
        telemetryInterval = TimeSpan.FromMilliseconds(interval);
    }

    return Task.CompletedTask;
}
~~~

### Reported properties

SimulatedDevice.cs
~~~ csharp
public async Task ReportConnectity()
{
    TwinCollection reportedProperties = new TwinCollection();
    TwinCollection connectivity = new TwinCollection();
    connectivity["type"] = "wifi";
    reportedProperties["connectivity"] = connectivity;

    await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
}
~~~

Wynik działania azure-iot-device-twin.json

~~~ json
 "reported": {
            "connectivity" :
            {
                "type" : "wifi"
            }
            }
        }

~~~

## Symulowanie Raspberry Pi

1. Wejdź na stronę **Raspberry Pi Azure IoT Online Simulator**
https://azure-samples.github.io/raspberry-pi-web-simulator/#GetStarted

1. Popraw connection string

~~~ javascript
const connectionString = '[your connection string]';
~~~

3. Naciśnij przycisk **Run**

## Symulowanie urządzenia w .NET Core

### Utworzenie aplikacji

~~~ bash
mkdir motorola-hackaton
cd motorola-hackaton
~~~

- Utworzenie projektu (automatycznie utworzy katalog)
~~~ bash
dotnet new console -o ./simulated-device/
~~~

- Otwarcie rozwiązania w Visual Studio Code
~~~ bash
code .
~~~

- Pobranie pakietów
~~~ bash
dotnet restore
~~~

- Zbudowanie projektu
~~~ bash
dotnet build
~~~

- Uruchomienie projektu
~~~ bash
dotnet run
~~~

- Utworzenie rozwiązania
~~~ bash
dotnet new sln
~~~

- Dodanie projektu do rozwiązania
~~~ bash
dotnet sln add ./simulated-device/simulated-device.csproj 
~~~

## Scenariusze w .NET Core

### Wysyłanie komunikatu z urządzenia do chmury (D2C)

SimulatedDevice.cs
~~~ csharp
 public async Task SendDeviceToCloudMessageAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            double minTemperature = 20;
            double minHumidity = 60;

            Random random = new Random();

            while(!cancellationToken.IsCancellationRequested)
            {
                double currentTemperature = minTemperature + random.NextDouble() * 15;
                double currentHumidity = minHumidity + random.NextDouble() * 20;

                var telemetryDataPoint = new 
                {
                    temperature = currentTemperature,
                    humidity = currentHumidity
                };

                string messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                await deviceClient.SendEventAsync(message, cancellationToken);
              
                System.Console.WriteLine($"{DateTime.Now} > Sending message: {messageString}");

                await Task.Delay(telemetryInterval, cancellationToken);
                
            }       
        }
~~~

 ### Odbieranie komunikatów z chmury na urządzeniu (C2D)

 ~~~ csharp
  public async Task ReceiveCloudToDeviceMessagesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Message receivedMessage = await deviceClient.ReceiveAsync();

                if (receivedMessage == null)
                {
                    continue;
                }

                var receivedJson = Encoding.UTF8.GetString(receivedMessage.GetBytes());

                Console.WriteLine($"Received message {receivedJson}");
        
                await deviceClient.CompleteAsync(receivedMessage);
            }
        }
~~~


### Rejestrowanie metod bezpośrednich (Direct)

SimulatedDevice.cs

~~~ csharp
 public async Task SetHandlers()
        {
             await deviceClient.SetMethodHandlerAsync(nameof(SetVolume), SetVolume, null);
        }

        public Task<MethodResponse> SetVolume(MethodRequest methhodRequest, object userContext)
        {
            string data = Encoding.UTF8.GetString(methhodRequest.Data);

            volume = byte.Parse(data);

            var response = new
            {
                result = $"Executed direct method {methhodRequest.Name}"
            };

            var result = JsonConvert.SerializeObject(response);

            MethodResponse methodResponse = new MethodResponse(Encoding.UTF8.GetBytes(result), 200);

            return Task.FromResult(methodResponse);

        }
~~~

### Wywoływanie metod bezpośrednich z chmury na urządzeniu 

Instalacja
~~~ bash
dotnet add package Microsoft.Azure.Devices
~~~

iot-hub-client.cs
~~~ csharp
 public async Task SetVolume(string deviceId, byte volume)
{ 
    CloudToDeviceMethod method = new CloudToDeviceMethod(nameof(SetVolume), TimeSpan.FromSeconds(10));
    method.SetPayloadJson(volume.ToString());

    System.Console.WriteLine($"Request {method.MethodName} {method.GetPayloadAsJson()}");

    var response = await serviceClient.InvokeDeviceMethodAsync(deviceId, method);
    
    var json = response.GetPayloadAsJson();

    System.Console.WriteLine($"Response {json}");
}
~~~

### Wysyłanie duzych plików z urządzenia do chmury (Blob)

SimulatedDevice.cs
~~~ csharp
public async Task SendDeviceToCloudBlobAsync(string filename, CancellationToken cancellationToken = default(CancellationToken))
{ 
    using (Stream stream = new FileStream(filename, FileMode.Open))
    {
        await deviceClient.UploadToBlobAsync(Path.GetFileName(filename), stream, cancellationToken);
    }
}
~~~


## Zarządzanie urządzeniami

Instalacja
~~~ bash
dotnet add package Microsoft.Azure.Devices
~~~

device-manager.cs

~~~ csharp
public class IotHubDevicesService : IDevicesService
    {
        private readonly RegistryManager registryManager;
        static string connectionString = "HostName=HackatonIoTHub1.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=75rcslOYtdBdIyDgpuykJe2EHHzo+rCfv/ZddmEdVKI=";

        public IotHubDevicesService()
            : this(RegistryManager.CreateFromConnectionString(connectionString))
        {
                    
        }
        public IotHubDevicesService(RegistryManager registryManager)
        {
            this.registryManager = registryManager;
        }

        public async Task<string> AddAsync(string deviceId)
        {
             Device device = await registryManager.AddDeviceAsync(new Device(deviceId));

            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        public async Task<string> GetAsync(string deviceId)
        {
            Device device = await registryManager.GetDeviceAsync(deviceId);

            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        // IoT Hub query language
        // https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-query-language
        public async Task<IEnumerable<string>> QueryAsync(string sql)
        {
            ICollection<string> devices = new List<string>();
            
            var query = registryManager.CreateQuery(sql, pageSize: 100);

            while (query.HasMoreResults)
            {
                var page = await query.GetNextAsTwinAsync();
                foreach (var twin in page)
                {
                    devices.Add(twin.DeviceId);
                }
            }

            return devices;
        }
    }
~~~

## Połączenie IoT DevKit AZ3166 to Azure IoT Hub

### Przyłączenie do sieci WiFi

1. Na płytce DevKit przytrzymaj **przycisk B**,  naciśnij i zwolnij przycisk **Reset**, a następnie zwolnij **przycisk B**. 
Zestaw DevKit przejdzie do trybu AP (Access Point). Na ekranie zostanie wyświetlony zostanie identyfikator SSID i adres IP.

1. Połącz się z komputera do sieci WIFI o identyfikatorze SSID wyświetlonym w poprzednim kroku. Hasło zostaw puste.

1. W przeglądarce wpisz adres _192.168.0.1_
Wybierz sieć Wi-Fi do której chcesz przyłączyć zestaw IoT DevKit, podaj hasło i naciśnij **Save**.

Teraz twój zestaw będzie łączyć się do podanej sieci WiFi.

### Konfiguracja urządzenia

1. Na płytce DevKit przytrzymaj **przycisk A**,  naciśnij i zwolnij przycisk **Reset**, a następnie zwolnij **przycisk A**. 
Zestaw DevKit przejdzie do konfiguracji.


### Wgrywanie aplikacji

1. W Visual Studio Code naciśnij przycisk **F1** i wpisz **Azure IoT Device Workbench: Upload Device Code**


Na podst.
https://docs.microsoft.com/pl-pl/azure/iot-hub/iot-hub-arduino-iot-devkit-az3166-get-started

## Stream Analytics

- Przykładowanie zapytanie 
~~~ sql
SELECT
    cast(iothub.EnqueuedTime as datetime) as event_time,
    cast(temperature as float) as temp
INTO
    outputblob
FROM
    inputiothub
    
~~~

- funkcje okienkowe
 https://docs.microsoft.com/pl-pl/azure/stream-analytics/stream-analytics-window-functions  

 ## Azure Module Edge

- Instalacja szablonu
~~~ bash
dotnet new -i Microsoft.Azure.IoT.Edge.Module
~~~

- Utworzenie projektu modułu IoT Edge
 ~~~ bash
 dotnet new aziotedgemodule
~~~

## Links
- IoT School
https://iotschool.microsoft.com

- Microsoft Azure IoT SDK for .NET
https://github.com/Azure/azure-iot-sdk-csharp