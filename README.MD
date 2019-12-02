This project bootstraps SPA apps with the benefit of integration with Spring Cloud Config and Spring Cloud Bus. It exposes a `/config` endpoint that will serve config values for the application with those in config server. 

It also supports integration with distributed configuration refresh messages via Spring Cloud Bus. This can be triggered via `actuator/bus-refresh` endpoint on either SPA app or any that is connected to the same app.

## How to use 

Set `SPRING__APPLICATION__NAME` environmental variable to assign name as configured in config-server (case sensitive).

### Compiling on Windows

```powershell
.\build.ps1
```


### Compiling on Linux or Mac

```bash
./build.sh
```

Final output will be placed in `/artifacts` folder

## How to Release

Use the embedded `Release` target to create and publish buildpack as asset to GitHub releases. This requires you have remote repo set and have specified API Key as parameter (or set via environmental variable).

