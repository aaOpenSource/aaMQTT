# aaMQTT
A small example of a bi-directional broker for MQTT to MXACCESS

##Proof of Concept
The current codeset is a working proof of concept showing how we can broker data between MQTT and Wonderware System Platform via the MXAccess protocol.  It is a complete example in the sense that it shows how you can both publish System Platform data to MQTT and subscribe to MQTT and write that data into Wonderware System platform.  What i have not done is create a complete example showing all options.  Nor have I spent any time performing any serious optimizations.  My intention is to plant a seed for others to take this work and push it even further. 

##Codesets
###Visual Studio
I have included a Visual Studio 2013 solution with C# code.  This code shows a complete example of publishing data to MQTT and also subscribing to data from MQTT.

###System Platform
If you are intimidated by Visual Studio then take a look at [SystemPlatformScript.txt](/SystemPlatformScript.txt).  I show you how you can publish data  to MQTT directly from System Platform scripts.  The limitation is that you can't subscribe to data.  The reason is that to receive the data you must register an asynchronous callback.  Due to the nature of the way quickscript works you can't register for callbacks.  If you know a way to accomplish this let me know and I'd be happy to update my code.

##Futures
At this point I don't have any major plans for feature adds in the future but if you would like to see something drop me a line and I'd be happy to look into it.

## Contributors
* [Andy Robinson](mailto:andy@phase2automation.com), Principal of [Phase 2 Automation](http://phase2automation.com).

## License
MIT License. See the [LICENSE file](/LICENSE) for details.

