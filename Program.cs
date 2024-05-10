using Microsoft.Extensions.Configuration;
using System.IO.Ports;
using System.Reflection;

/*
{
  "serial": {
    "port" : "COM4"
  },
  "log":{
    "path": "c:/temp/log"
  }
}
*/
Console.WriteLine("Lecture config");
var executableFileName = Environment.ProcessPath;
var jsonFileName = Path.ChangeExtension(executableFileName, ".json");
if (jsonFileName == null) 
{
    Console.WriteLine("Erreur: pas de non d'exécutable.");
    return;
}
var config = new ConfigurationBuilder().AddJsonFile(jsonFileName, true, false).Build();
var serialPortName = config["serial:port"];
if (serialPortName == null) 
{
    serialPortName = "COM4";
}
var logPath = config["log:path"];
if (logPath != null) 
{
    Console.WriteLine($"Chemin du log {logPath}'");
}
Console.WriteLine($"Démarrage du monitoring avec le port série '{serialPortName}'");

var sp = new SerialPort() { 
    PortName = serialPortName,
    //BaudRate = 115200,
    //Parity = Parity.Odd,
    ReadTimeout = 100
};
sp.Open();

var loop = true;
while (loop)
{
    try
    {
        // Lecture en continue caractère par caractère
        // Si lecture avec un retour chariot comme marqueur de fin => ReadLine
        char data = (char)sp.ReadChar();
        Console.Write(data);
        if (logPath != null)
        {
            var dateTime = DateTime.Now;
            var logFileName = $"log_{dateTime.ToString("yyyy-MM-dd")}";
            var fullLogFileName = Path.Combine(logPath, logFileName);
            if (!File.Exists(fullLogFileName))
            {
                File.Create(fullLogFileName).Close();
            }
            using (StreamWriter writer = File.AppendText(fullLogFileName))
            {
                writer.Write(data);
            }
        }
    }
    catch (TimeoutException) { }
    catch (InvalidOperationException) 
    {
        break;
    }

}

sp.Close();
Console.WriteLine("Arrêt du monitoring");