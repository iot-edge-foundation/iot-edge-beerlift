# iot-edge-beerlift
Beer lift connected to the Azure cloud using Azure IoT edge.

*Note*: This is still a work in progress. 

# Introduction

This project involves a beer lift that will pop out of the floor when summoned.

The beer lift is connected to the Azure cloud using an Azure IoT Edge module running of a Raspberry Pi. 

The lift can be moved up and down from the cloud. 

The lift also measures the temperature and it measures which bottles are taken out. Each bottle has its own unique position, guided by LEDs. 

This beer lift is designed for up to 16 bottles.

# Materials used

## For the Beer Lift
* thread screw bar (1+ meter)
* Lenze electromotor with gearbox - type SSN31-1UHAR-056C21 (second hand) (340V 3 Phase downscaled to 220V 1 Phase)
* Capacitor 12.0uF (for switching back to 1 phase)
* Two relays, Siemens Sirius 3RT1015-1BB41 (For switching 220V on the motor, one in each direction)
* Two Snap-Action Switches with roller Lever; 3-Pin (for normally closed usage)
* Wires, Wago connectors, etc.

## For the 16 beerholders

* a LED
* a Snap-Action switch

## For the controller
* Raspberry Pi 3 with Buster
* RPi GPIO extension connector and flat cable
* Two-channel 5V relais module
* DHT22 AM2302 Digital temperature and humidity sensor
* One 10K resistor for the DHT22
* Two MCP23017 - i2c 16 input/output port expanders
* Prototyping PCB boards, wires, etc


# Azure IoT Edge

The Azure IoT Edge module is available [here](https://hub.docker.com/repository/docker/svelde/iot-edge-beerlift).

This repo is only available for Raspberry PI (ARM-32) due to the usage of GPIO.

## Telemetry

Every 'Interval' milliseconds, the beerlift is inspected for changes in the bottle holders. at the same time, the state of the beerlift is checked.

The state can be:

* Down
* Going up
* Up
* Going down
* Unknown or exception state

If any bottles are taken out (or refilled) or the state is changes, a message is send.

Message:

```
class BeerLiftMessage
{
    bool BeerState01 {get; set;}
    bool BeerState02 {get; set;}
    bool BeerState03 {get; set;}
    bool BeerState04 {get; set;}
    bool BeerState05 {get; set;}
    bool BeerState06 {get; set;}
    bool BeerState07 {get; set;}
    bool BeerState08 {get; set;}
    bool BeerState09 {get; set;}
    bool BeerState10 {get; set;}
    bool BeerState11 {get; set;}
    bool BeerState12 {get; set;}
    bool BeerState13 {get; set;}
    bool BeerState14 {get; set;}
    bool BeerState15 {get; set;}
    bool BeerState16 {get; set;}
    DateTime Timestamp {get; set;}
    string State {get; set;}
}
```

## Direct Methods

The following direct methods are available:

* Up
* Down
* Ambiant

*Note* Method names are case sensitive

#### Up

No JSON body needed to be send.

Sends the lift up for the duration of 'UpDownInterval' milliseconds (20000 by default).

*Note*: due to the time to complete this action, a time out can occur.

Response:

```
class UpResponse 
{
    int responseState { get; set; }
    string errorMessage { get; set; }
}
```

### Down

No JSON body needed to be send.

Sends the lift down for the duration of 'UpDownInterval' milliseconds (20000 by default).

*Note*: due to the time to complete this action, a time out can occur.

Response:

```
class DownResponse 
{
    int responseState { get; set; }
    string errorMessage { get; set; }
}
```

### Ambiant

No JSON body needed to be send.

Reads the temperature and humidity of the DHT22.

Due to the behavior of the DHT, multiple reads are needed unit a actual temperature is read. The humidity is not valid, please ignore.

The state of the lift can be seen too. 

Response:

```
class AmbiantValuesResponse 
{
    int responseState { get; set; }
    string errorMessage { get; set; }
    double Temperature {get; set;}
    double Humidity {get; set;}    
    string State {get; set;}
}
```

# Links

The MCP23017 access is based on library https://github.com/dotnet/iot/tree/master/src/devices/Mcp23xxx
The DHT22 access is based on library https://github.com/dotnet/iot/tree/master/src/devices/Dhtxx 
