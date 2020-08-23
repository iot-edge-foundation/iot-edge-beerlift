# iot-edge-beerlift
Beer lift connected to the Azure cloud using Azure IoT edge.

*Note*: This is still a work in progress. 

# Materials used

## For the Beer Lift
* thread screw bar (1+ meter)
* Lenze electromotor with gearbox - type SSN31-1UHAR-056C21 (second hand) (340V 3 Phase downscaled to 220V 1 Phase)
* Capacitor 12.0uF (for switching back to 1 phase)
* Two relays, Siemens Sirius 3RT1015-1BB41 (For switching 220V on the motor, one in each direction)
* Two Snap-Action Switches with roller Lever; 3-Pin (for normally closed usage)
* Wires, Wago connectors, etc.

## For the controller
* Raspberry Pi 3
* RPi GPIO extension connector and flat cable
* Two-channel 5V relais module
* DHT22 AM2302 Digital temperature and humidity sensor
* One 10K resistor for the DHT22
* Two MCP23017 - i2c 16 input/output port expanders
* Prototyping PCB boards, wires, etc

# Azure IoT Edge

The Azure IoT Edge module is available [here](https://hub.docker.com/repository/docker/svelde/iot-edge-beerlift).

This repo is only available for Raspberry PI (ARM-32) due to the usage of GPIO.

## Direct Methods

#### Up

No JSON body needed to be send.

Sends the lift up for the duration of 'UpDownInterval' milliseconds (20000 by default).

*Note*: due to the time to complete this action, a time out can occur.

Response:

```
public class UpResponse 
{
    public int responseState { get; set; }

    public string errorMessage { get; set; }
}
```

### Down

No JSON body needed to be send.

Sends the lift down for the duration of 'UpDownInterval' milliseconds (20000 by default).

*Note*: due to the time to complete this action, a time out can occur.

Response:

```
public class DownResponse 
{
    public int responseState { get; set; }

    public string errorMessage { get; set; }
}
```

### Ambiant

Reads the temperature and humidity of the DHT22.

Due to the behavior of the DHT, multiple reads are needed unit a actual temperature is read. The humidity is not valid, please ignore.

Response:

```
public class AmbiantValuesResponse 
{
    public int responseState { get; set; }

    public string errorMessage { get; set; }

    public double Temperature {get; set;}

    public double Humidity {get; set;}
}
```

# Links

The MCP23017 access is based on library https://github.com/dotnet/iot/tree/master/src/devices/Mcp23xxx
The DHT22 access is based on library https://github.com/dotnet/iot/tree/master/src/devices/Dhtxx 
