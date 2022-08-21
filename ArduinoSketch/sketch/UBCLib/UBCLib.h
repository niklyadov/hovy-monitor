#pragma once
#include <Arduino.h>

static const byte ArgsTypeNoArgs = 0x00;
static const byte ArgsTypeInt = 0x01;
static const byte ArgsTypeString = 0x03;
static const byte ArgsTypeIntPair = 0x04;

class Message {
	public:
		Message();

		byte bytes[128];
		byte size;
		byte argsType;
		String command;
		bool isFinished;

		bool tryGetIntArg(int& out);
		bool tryGetStringArg(String& out);
		bool tryGetIntPairArg(int& out_x, int& out_y);
		
		void writeByte(byte b);
		void reset();
	private:
		byte _index = 0;		
		void setCommand(String command_name);
};

extern "C" {
  typedef void (*func)(Message);
}

class UBCSerial {
	public:
		void begin(int speed);
		void onMessageReceived(func f);
		void read();
		void send(Message& message);
		void send(String command_name);
		void send(String command_name, int arg);
		void send(String command_name, String arg);
		void send(String command_name, int x, int y);
	private:
		func _onMessageReceived;
		Message _message;
};