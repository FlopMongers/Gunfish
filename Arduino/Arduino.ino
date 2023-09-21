/*This is my crack at a state-based approach to automating a Big Mouth Billy Bass.
 This code was built on work done by both Donald Bell and github user jswett77. 
 See links below for more information on their previous work.

 In this code you'll find reference to the MX1508 library, which is a simple 
 library I wrote to interface with the extremely cheap 2-channel H-bridges that
 use the MX1508 driver chip. It may also work with other H-bridges that use different
 chips (such as the L298N), so long as you can PWM the inputs.

 This code watches for a voltage increase on input A0, and when sound rises above a
 set threshold it opens the mouth of the fish. When the voltage falls below the threshold,
 the mouth closes.The result is the appearance of the mouth "riding the wave" of audio
 amplitude, and reacting to each voltage spike by opening again. There is also some code
 which adds body movements for a bit more personality while talking.

 Most of this work was based on the code written by jswett77, and can be found here:
 https://github.com/jswett77/big_mouth/blob/master/billy.ino

 Donald Bell wrote the initial code for getting a Billy Bass to react to audio input,
 and his project can be found on Instructables here:
 https://www.instructables.com/id/Animate-a-Billy-Bass-Mouth-With-Any-Audio-Source/

 Author: Jordan Bunker <jordan@hierotechnics.com> 2019
 License: MIT License (https://opensource.org/licenses/MIT)
*/

#if (ARDUINO >= 100)
#include "Arduino.h"
#else
#include "WProgram.h"
#endif

class MX1508 {
  public:
    // Constructor
    MX1508(int pin1, int pin2);

    // Methods
    void forward();
    void backward();
    void setSpeed(int motorSpeed);
    void halt();

  private:
    int _pin1;
    int _pin2;
    int _motorSpeed;
};

MX1508::MX1508(int pin1, int pin2) {
  pinMode(pin1, OUTPUT);
  _pin1 = pin1;
  pinMode(pin2, OUTPUT);
  _pin2 = pin2;
}

void MX1508::forward() {
  analogWrite(_pin1, _motorSpeed);
  digitalWrite(_pin2, LOW);
}

void MX1508::backward() {
  digitalWrite(_pin1, LOW);
  analogWrite(_pin2, _motorSpeed);
}

void MX1508::setSpeed(int motorSpeed) {
  _motorSpeed = motorSpeed;
}

void MX1508::halt() {
  digitalWrite(_pin1, LOW);
  digitalWrite(_pin2, LOW);
}

MX1508 bodyMotor(6, 9); // Sets up an MX1508 controlled motor on PWM pins 6 and 9
MX1508 mouthMotor(5, 3); // Sets up an MX1508 controlled motor on PWM pins 5 and 3

int soundPin = A0; // Sound input

int silence = 2; // Threshold for "silence". Anything below this level is ignored.
int soundVolume = 0; // variable to hold the analog audio value

//these variables are for storing the current time, scheduling times for actions to end, and when the action took place
unsigned long currentTime;
unsigned long timeOfLastSound;
unsigned long timeOfLastByte;
unsigned long mouthActionTime;

enum FishState {
  WAIT,
  TALK
};
enum MouthState {
  MOUTH_OPENING,
  MOUTH_CLOSING
};

FishState fishState = WAIT;
MouthState mouthState = MOUTH_CLOSING;

void setup() {
//Max speeds of the motors
  bodyMotor.setSpeed(255); 
  mouthMotor.setSpeed(120);

  timeOfLastSound = millis();
  timeOfLastByte = millis();
  mouthActionTime = millis();

//input mode for sound pin
  pinMode(soundPin, INPUT);
  pinMode(LED_BUILTIN, OUTPUT);

  Serial.begin(9600);
}

void loop() {
  currentTime = millis();
  updateSoundInput();
  handleState();
  handleMouthState();
  delay(10);
}

void handleState() {
  if (fishState == WAIT) {
    wait();
  } else {
    talk();
  }
}

void talk() {
  // Transition
  if (millis() - timeOfLastSound > 1000) {
    fishState = WAIT;
    bodyMotor.halt();
    // digitalWrite(LED_BUILTIN, LOW);
    return;
  }

  if (soundVolume > silence) {
    timeOfLastSound = millis();
  }
}

void wait() {
  // Transition
  if (soundVolume > silence) {
    timeOfLastSound = 0;
    fishState = TALK;
    bodyMotor.forward();
    // digitalWrite(LED_BUILTIN, HIGH);
    
    return;
  }
}

void handleMouthState() {
  if (mouthState == MOUTH_OPENING) {
    mouthOpening();
  } else {
    mouthClosing();
  }
}

void mouthOpening() {
  if (currentTime > mouthActionTime) {
    mouthState = MOUTH_CLOSING;
    mouthMotor.halt();
    digitalWrite(LED_BUILTIN, LOW);
    mouthActionTime = currentTime + 400;
    return;
  }
}
void mouthClosing() {
  if ((currentTime > mouthActionTime) && (fishState == TALK)) {
    mouthState = MOUTH_OPENING;
    mouthMotor.forward();
    digitalWrite(LED_BUILTIN, HIGH);
    mouthActionTime = currentTime + 100;
    return;
  }
}

void updateSoundInput() {
  if (Serial.available() > 0) {
    int value = Serial.read();
    if (value >= 0)
    {
      soundVolume = value;
      timeOfLastByte = currentTime;
    }
  } else if (currentTime > timeOfLastByte + 1000) {
    soundVolume = 0;
  }
}
