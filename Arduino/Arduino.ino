#if (ARDUINO >= 100)
#include "Arduino.h"
#else
#include "WProgram.h"
#endif

class Motor {
public:
  // Constructor
  Motor(int pin1, int pin2);

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

Motor::Motor(int pin1, int pin2) {
  pinMode(pin1, OUTPUT);
  _pin1 = pin1;
  pinMode(pin2, OUTPUT);
  _pin2 = pin2;
}

void Motor::forward() {
  digitalWrite(_pin1, HIGH);
  digitalWrite(_pin2, LOW);
}

void Motor::backward() {
  digitalWrite(_pin1, LOW);
  digitalWrite(_pin2, HIGH);
}

void Motor::setSpeed(int motorSpeed) {
  _motorSpeed = motorSpeed;
}

void Motor::halt() {
  digitalWrite(_pin1, LOW);
  digitalWrite(_pin2, LOW);
}

Motor bodyMotor(12, 7);   // Sets up an Motor controlled motor on PWM pins 6 and 9
Motor mouthMotor(10, 8);  // Sets up an Motor controlled motor on PWM pins 5 and 3

int silence = 20;  // Threshold for "silence". Anything below this level is ignored.
int volume = 0;    // variable to hold the analog audio value

//these variables are for storing the current time, scheduling times for actions to end, and when the action took place
long currentTime;
long timeOfLastSound;
long mouthActionDuration;
long mouthActionTime;
long mouthActionHalfTime;

bool bodyUp;
long bodyUpTime;
long bodyDownTime;

enum FishState {
  WAIT,
  TALK
};

FishState fishState = WAIT;

bool debug;

void setup() {
  debug = false;

  timeOfLastSound = millis();

  pinMode(LED_BUILTIN, OUTPUT);

  Serial.begin(57600);
}

void loop() {
  currentTime = millis();
  volume = getVolumeFromSerial();

  if (debug) { 
    debugMovement();
  } else {
    handleState();
    articulateBody();
    digitalWrite(LED_BUILTIN, (int)bodyUp);
  }
}

void debugMovement() {
  if (currentTime % 2000 > 1000) {
    bodyMotor.forward();
    mouthMotor.forward();
    digitalWrite(LED_BUILTIN, 0);
  } else {
    mouthMotor.halt();
    bodyMotor.halt();
    digitalWrite(LED_BUILTIN, 1);
  }
}

void handleState() {
  if (fishState == WAIT) {
    wait();
  } else {
    talk();
  }
}

void articulateBody() {
  if (bodyUp) {
    if (currentTime - bodyUpTime < 1000) {
      bodyMotor.forward();
    } else {
      bodyMotor.halt();
    }
  } else {
    if (currentTime - bodyDownTime < 900) {
      bodyMotor.backward();
    } else {
      bodyMotor.halt();
    }
  }
}

void talk() {
  // Transition
  if (currentTime - timeOfLastSound > 100) {
    mouthMotor.halt();
    fishState = WAIT;
    return;
  }

  // Action
  if (currentTime < mouthActionTime) {
    if (currentTime < mouthActionHalfTime) {
      mouthMotor.forward();
    } else {
      mouthMotor.backward();
    }
  } else {
    mouthActionDuration = floor(random(150, 300));
    mouthActionTime = currentTime + mouthActionDuration;
    mouthActionHalfTime = currentTime + (mouthActionDuration >> 1);
  }

  if (volume > silence) {
    timeOfLastSound = currentTime;
  }
}

void wait() {
  // Transition
  if (volume > silence) {
    timeOfLastSound = currentTime;
    mouthActionTime = currentTime;
    bodyUp = true;
    bodyUpTime = currentTime;
    fishState = TALK;
    return;
  }

  if (bodyUp && currentTime - timeOfLastSound > 2000) {
    bodyUp = false;
    bodyDownTime = currentTime;
  }

  // Action
  bodyMotor.halt();
  mouthMotor.halt();
}

int getVolumeFromSerial() {
  if (Serial.available()) {
    unsigned char value = Serial.read();
    return (int)value;
  } else {
    return 0;
  }
}
