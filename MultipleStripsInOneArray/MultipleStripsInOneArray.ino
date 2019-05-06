// MultipleStripsInOneArray - see https://github.com/FastLED/FastLED/wiki/Multiple-Controller-Examples for more info on
// using multiple controllers.  In this example, we're going to set up four NEOPIXEL strips on three
// different pins, each strip will be referring to a different part of the single led array
#include<Uduino.h>
Uduino uduino("MassSpec");

#include <FastLED.h>

#define NUM_STRIPS 4
#define NUM_LEDS_PER_STRIP 22
#define NUM_LEDS NUM_LEDS_PER_STRIP * NUM_STRIPS

#define UPDATES_PER_SECOND 5000

CRGBPalette16 currentPalette;
TBlendType    currentBlending;

CRGB leds[NUM_STRIPS * NUM_LEDS_PER_STRIP];

bool machineOn = false;
// For mirroring strips, all the "special" stuff happens just in setup.  We
// just addLeds multiple times, once for each strip
void setup() {
  Serial.begin(9600);
  // tell FastLED there's 60 NEOPIXEL leds on pin 10, starting at index 0 in the led array
  FastLED.addLeds<NEOPIXEL, 8>(leds, 0, NUM_LEDS_PER_STRIP);

  // tell FastLED there's 60 NEOPIXEL leds on pin 11, starting at index 60 in the led array
  FastLED.addLeds<NEOPIXEL, 9>(leds, NUM_LEDS_PER_STRIP, NUM_LEDS_PER_STRIP);

  // tell FastLED there's 60 NEOPIXEL leds on pin 12, starting at index 120 in the led array
  FastLED.addLeds<NEOPIXEL, 10>(leds, 2 * NUM_LEDS_PER_STRIP, NUM_LEDS_PER_STRIP);

  // tell FastLED there's 60 NEOPIXEL leds on pin 12, starting at index 120 in the led array
  FastLED.addLeds<NEOPIXEL, 11>(leds, 3 * NUM_LEDS_PER_STRIP, NUM_LEDS_PER_STRIP);
  //SetupBlackAndWhiteStripedPalette();
  fill_solid( currentPalette, 16, CRGB::Black);
  currentBlending = LINEARBLEND;
  //machineOn = true;
  uduino.addCommand("turnOn", turnOnMachine);
  uduino.addCommand("turnOff", turnOffMachine);
}

void turnOnMachine(){
  machineOn = true;
  Serial.print("Turn On Machine");
  SetupBlackAndWhiteStripedPalette();
}

void turnOffMachine(){
  machineOn = false;
  Serial.print("Turn On Machine");
  fill_solid( currentPalette, 16, CRGB::Black);
}

void loop() {
  uduino.update();
  delay(10);
  static uint8_t startIndex = NUM_LEDS;
    startIndex = startIndex - 1; /* motion speed */
    
    FillLEDsFromPaletteColors( startIndex);
    
    FastLED.show();
    FastLED.delay(1000 / UPDATES_PER_SECOND);
}

void SetupBlackAndWhiteStripedPalette()
{
    // 'black out' all 16 palette entries...
    fill_solid( currentPalette, 16, CRGB::Black);
    // and set every fourth one to white.
    currentPalette[0] = CRGB::White;
    currentPalette[4] = CRGB::White;
    currentPalette[8] = CRGB::White;
    currentPalette[12] = CRGB::White;
    
}

void FillLEDsFromPaletteColors( uint8_t colorIndex)
{
    uint8_t brightness = 255;
    
    for( int i = 0; i < NUM_LEDS; i++) {
        leds[i] = ColorFromPalette( currentPalette, colorIndex, brightness, currentBlending);
        colorIndex += 3;
    }
}
