# Log4NetWithRabbitMq
Project for extracting a nuget package that enables the utilization
of Log4Net for logging exceptions and RabbitMQ for queueing and
consuming messages from queues.

## **Publisher config**
```xml
<configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
</configSections>
<log4net>
  <appender name="RabbitMqAppender" type="HSG.Exception.Logging.Appender.RabbitMqAppender, HSG.Exception.Logging">
    <HostName value="localhost" />
    <VirtualHost value="/" />
    <UserName value="guest" />
    <Password value="testtest" />
    <Port value="5672" />
    <FlushInterval value="5" />
    <Tenent value="p.s.k" />
    <Environment value="prod.uction" />
    <AppName value="livebet" />
    <DepthOfLog value="2" />
    <ExchangeName value="HattrickExchange" />
  </appender>
  <logger name="ExceptionLogger" additivity="false">
      <level value="ALL" />
      <appender-ref ref="RabbitMqAppender" />
  </logger>
</log4net>
```

 |KEY          |DEFAULT VALUE        |DESCRIPTION |
   | -------------  |:-------------: | ----------- |
   |HostName        |localhost       |Host to establish connection with.|   
   |VirtualHost     |/               |A host can be separated into multiple virtual hosts.|   
   |UserName        |guest           |Username to use for credentials.|   
   |Password        |guest           |Password to use for credentials.|  
   |Port            |5672            |Port to use for communication.| 
   |FlushInterval   |5               |Seconds to wait between message send.| 
   |Tenent          |""              |Application tenent.|
   |Environment     |""              |Application environment.|   
   |AppName         |""              |Application name.|  
   |DepthOfLog      |0               |How deep will inner exceptions be logged. 0 means only top level <br> exception is logged.|
   |ExchangeName    |HattrickExchange|Name of exchange to declare and connect to.
   
   Default values can be omitted from config.
   
   **WARNING!** Periods can be used in Tenent/Environment/AppName but they will be replaced
   with %2E to prevent RabbitMQ from thinking they're a routing key separator.
   So if there are periods in those values, be sure to subscribe to the appropriate
   queues using %2E in the words, leaving periods only as a separator.
   
   *e.g. ca.sa.production.betshop.DEBUG should be: ca%2Esa.production.betshop.DEBUG*
   
## **Publisher usage**
```C#
log4net.Config.BasicConfigurator.Configure();
var logger = LogManager.GetLogger("ExceptionLogger");
logger.Debug(e); // or some other level's method, e is an exception
```
### **Default levels**
1. **Debug**
2. **Info**
3. **Warn**
4. **Error**
5. **Fatal**

## **Consumer config**
Reuse config from publisher, or add key value pairs to appSettings:
```xml
<appSettings>
  <add key="HostName" value="localhost" />
  <add key="VirtualHost" value="/" />
  <add key="UserName" value="guest" />
  <add key="Password" value="testtest" />
  <add key="Port" value="5672" />
  <add key="ExchangeName" value="HattrickExchange" />
</appSettings>
```
## **Consumer usage**
```C#
var repo = new ConsumerRepo(); // declare consumer repository
```
**Methods:**
```C#
DeclareQueue(string queueName, bool willDeleteAfterConnectionClose, IEnumerable<string> routingKeys)
```
Specify name of queue to declare, whether it should be exclusive (delete on connection shutdown),
and the bindings of the queue (specific routing keys to subscribe to).
```C#    
ConnectToQueue(string queueToConnectToName, int numberOfThreads=1)
```
Connect to queue specified by name and start consuming. Specify number of threads to set up to
achieve load balance (worker threads). Default number of threads is 1.
```C#    
DisconnectFromQueue(string consumerToDisconnectTag)
```
Disconnects consumer with specified tag from its queue. Each consumer's tag is specified at creation time.
```C#
DeleteQueue(string queueToDeleteName)
```
Deletes the queue specified by the name.
```C#    
CloseConnection()
```
Closes the whole connection. Therefore all consumers die too.
