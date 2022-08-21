#include <UBCLib.h>

void UBCSerial::begin(int speed)
{
	Serial.begin(speed);
}

void UBCSerial::read()
{
	while (Serial.available())
	{
		_message.writeByte(Serial.read());
		if (_message.isFinished)
		{
			_onMessageReceived(_message);
			break;
		}
		delay(10);
	}
	_message.reset();
}

void UBCSerial::onMessageReceived(func f)
{
	_onMessageReceived = f;
}

void UBCSerial::send(String command_name)
{
	Serial.write(10);
	Serial.write(ArgsTypeNoArgs);
	for (int i = 0; i < sizeof(command_name); i++)
	{
		Serial.write(command_name[i]);
	}
	for (int i = sizeof(command_name); i < 8; i++)
	{
		Serial.write(0);
	}
}

void UBCSerial::send(String command_name, int arg)
{
	Serial.write(14);
	Serial.write(ArgsTypeInt);
	for (int i = 0; i < sizeof(command_name); i++)
	{
		Serial.write(command_name[i]);
	}
	for (int i = sizeof(command_name); i < 8; i++)
	{
		Serial.write(0);
	}
	for (int i = 3; i > -1; i--)
	{
		Serial.write(arg >> (i * 8));
	}
}

void UBCSerial::send(String command_name, String arg)
{
	Serial.write(10 + sizeof(arg));
	Serial.write(ArgsTypeString);
	for (int i = 0; i < sizeof(command_name); i++)
	{
		Serial.write(command_name[i]);
	}
	for (int i = sizeof(command_name); i < 8; i++)
	{
		Serial.write(0);
	}
	for (int i = 0; i < sizeof(arg); i++)
	{
		Serial.write(arg[i]);
	}
}

void UBCSerial::send(String command_name, int x, int y)
{
	Serial.write(18);
	Serial.write(ArgsTypeIntPair);
	for (int i = 0; i < sizeof(command_name); i++)
	{
		Serial.write(command_name[i]);
	}
	for (int i = sizeof(command_name); i < 8; i++)
	{
		Serial.write(0);
	}
	for (int i = 3; i > -1; i--)
	{
		Serial.write(x >> (i * 8));
	}
	for (int i = 3; i > -1; i--)
	{
		Serial.write(y >> (i * 8));
	}
}
