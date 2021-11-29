NLog.HttpClient
==========

An implementation of the NLog by sending the log using HttpClient in custom functionalities.

------------

Usage
=====
1. Configure NLog to use `NLog.HttpClient`: https://github.com/nlog/nlog/wiki/Configuration-file

### Example NLog.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      xsi:schemaLocation="http://www.nlog-project.org/schemas/NLog.xsd NLog.xsd"
      autoReload="true"
      throwExceptions="true">
  <extensions>
    <add assembly="NLog.HttpClient" />
  </extensions>
  <targets>
    <target xsi:type="File" name="f" fileName="${basedir}/logs/${shortdate}.log" layout="${longdate} ${uppercase:${level}} ${message}" />
    <target name="http_test" xsi:type="AsyncWrapper">
      <target
              xsi:type="HttpClient"
              Url="https://localhost:5001/api/logs"
              Auth="2123"
      >
        <field name="Level" layout="${level}"/>
        <field name="Message" layout="${message}" />
        <field name="Logger" layout="${logger}"/>
        <field name="Exception" layout="${exception:format=tostring}" />
        <field name="ThreadID" layout="${threadid}" dataType="Int32" />
        <field name="ThreadName" layout="${threadname}" />
        <field name="ProcessID" layout="${processid}" dataType="Int32" />
        <field name="ProcessName" layout="${processname:fullName=true}" />
        <field name="UserName" layout="${windows-identity}" />
      </target>
    </target>

  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="f" />
    <logger name="*" minlevel="Debug" writeTo="http_test" />
  </rules>
</nlog>
```
---------
### Web API
```csharp
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        [HttpPost]
        public void Post([FromBody] object value)
        {
            var str = JsonConvert.SerializeObject(value);
            Debug.WriteLine(str);
        }
    }
```

### Client
```csharp
    class Program
    {
        static void Main(string[] args)
        {
            var log = LogManager.GetCurrentClassLogger();
            while (true)
            {
                log.Debug(Console.ReadLine());
            }
        }
    }
```


### Configuration Options

Key        | Description
----------:| -----------
Url        | Base URL of your Web API
Auth       | Authentication Token for Bearer token
field      | Array of filed in json format ke/value pair

----------
Reference: https://github.com/narfunikita/NLog.Telegram