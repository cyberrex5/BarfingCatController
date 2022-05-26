#include <SoftwareSerial.h>
#include <Servo.h>

// servo angles, determined by experimenting
#define SA_FORWARD 82
#define SA_LEFT 178
#define SA_RIGHT 0

const uint8_t servoPin = 9;

const uint8_t echoPin = 4;
const uint8_t trigPin = 5;

const uint8_t rMotorPos = 12;
const uint8_t rMotorNeg = 13;
const uint8_t lMotorPos = 7;
const uint8_t lMotorNeg = 8;
const uint8_t rMotorEn = 11;
const uint8_t lMotorEn = 6;

SoftwareSerial BtSerial(3, 2); // RX, TX

Servo servo;

int normalSpeed = 100; // 0 - 255

int minObjectDist = 15;

unsigned long lastDistCheckTime = 0;

char prevMsg = '\0';

void setup()
{
    servo.attach(servoPin);

    pinMode(echoPin, INPUT);
    pinMode(trigPin, OUTPUT);

    pinMode(rMotorPos, OUTPUT);
    pinMode(rMotorNeg, OUTPUT);
    pinMode(lMotorPos, OUTPUT);
    pinMode(lMotorNeg, OUTPUT);

    pinMode(rMotorEn, OUTPUT);
    pinMode(lMotorEn, OUTPUT);

    setSpeed(normalSpeed);

    Serial.begin(9600);
    BtSerial.begin(9600);

    servo.write(SA_FORWARD);
}

void loop()
{
    if (BtSerial.available() > 0)
    {
        switch (prevMsg)
        {
            case 'n':
                normalSpeed = BtSerial.read();
                setSpeed(normalSpeed);
                Serial.println("ser normal speed to: " + normalSpeed);
                prevMsg = '\0';
                return;

            case 'm':
                minObjectDist = BtSerial.read();
                Serial.println("ser normal speed to: " + minObjectDist);
                prevMsg = '\0';
                return;
        }

        char msg = (char)BtSerial.read();
        switch (msg)
        {
            case 'w':
                forward();
                break;

            case 'a':
                left();
                break;

            case 's':
                back();
                break;

            case 'd':
                right();
                break;

            case 'f':
                stoop();
                break;

            case 'r':
                servo.write(SA_RIGHT);
                break;
            
            case 'l':
                servo.write(SA_LEFT);
                break;

            case 'z':
                servo.write(SA_FORWARD);
                break;
        }
        prevMsg = msg;
    }

    if (millis() - lastDistCheckTime > 100)
    {
        int objDist = getDistFromUltrasonic();

        if (objDist < minObjectDist)
        {
            BtSerial.println("o");
        }
        else
        {
            // BtSerial.println("p");
        }

        lastDistCheckTime = millis();
    }
}

// returns: distance in cm from ultrasonic sensor
int getDistFromUltrasonic()
{
  digitalWrite(trigPin, LOW);
  delayMicroseconds(5);
  digitalWrite(trigPin, HIGH);
  delayMicroseconds(10);
  digitalWrite(trigPin, LOW);
  
  unsigned long duration = pulseIn(echoPin, HIGH);
  return int(duration * 0.01715); // 0.01715 == 0.0343 (speed of sound) / 2; because the sound wave travels from to the object and back
}

void forward()
{
    setSpeed(normalSpeed);
    digitalWrite(rMotorPos, HIGH);
    digitalWrite(rMotorNeg, LOW);
    digitalWrite(lMotorPos, HIGH);
    digitalWrite(lMotorNeg, LOW);
}

void back()
{
    setSpeed(normalSpeed);
    digitalWrite(rMotorPos, LOW);
    digitalWrite(rMotorNeg, HIGH);
    digitalWrite(lMotorPos, LOW);
    digitalWrite(lMotorNeg, HIGH);
}

void left()
{
    setSpeed(normalSpeed);
    digitalWrite(rMotorPos, HIGH);
    digitalWrite(rMotorNeg, LOW);
    digitalWrite(lMotorPos, LOW);
    digitalWrite(lMotorNeg, HIGH);
}

void right()
{
    setSpeed(normalSpeed);
    digitalWrite(rMotorPos, LOW);
    digitalWrite(rMotorNeg, HIGH);
    digitalWrite(lMotorPos, HIGH);
    digitalWrite(lMotorNeg, LOW);
}

void stoop()
{
    digitalWrite(rMotorPos, LOW);
    digitalWrite(rMotorNeg, LOW);
    digitalWrite(lMotorPos, LOW);
    digitalWrite(lMotorNeg, LOW);
}

void setSpeed(int speed)
{
    analogWrite(rMotorEn, speed);
    analogWrite(lMotorEn, speed);
}
