#include <UBCLib.h>

Message::Message(){}

void Message::writeByte(byte b) 
{
	bytes[_index] = b;
	
	if (_index == 0) {
		command = "";
		isFinished = false;
		size = b;
	} else if (_index == 1) {
		argsType = b;
	} else if (_index < 10) {
		if (b != 0) {
			command += char(b);
		}
	}
	
	if (_index + 1 >= size){
		isFinished = true;
	}
	_index++;
	
	if (isFinished or _index == 128) {
		_index = 0;
	}
}

void Message::reset()
{
	_index = 0;
}

bool Message::tryGetIntArg(int& out)
{
	if (argsType == ArgsTypeInt && size >= 14)
	{
		out = int(bytes[10] << 24 |
				  bytes[11] << 16 |
				  bytes[12] << 8  |
				  bytes[13]);
		return true;
	}
	return false;
}

bool Message::tryGetStringArg(String& out)
{
	if (argsType == ArgsTypeString)
	{
		for (int i = 10; i < size; i++)
		{
			out += char(bytes[i]);
		}
		return true;
	}
	return false;
}

bool Message::tryGetIntPairArg(int& out_x, int& out_y)
{
	if (argsType == ArgsTypeIntPair && size >= 18)
	{
		out_x = int(bytes[10] << 24 |
				  bytes[11] << 16 |
				  bytes[12] << 8  |
				  bytes[13]);
		out_y = int(bytes[14] << 24 |
				  bytes[15] << 16 |
				  bytes[16] << 8  |
				  bytes[17]);
		return true;
	}
	return false;
}

void Message::setCommand(String command_name)
{
	command = command_name;
	for (int i = 0; i < 8; i++)
	{
		bytes[2 + i] = (byte)command_name.charAt(i);
	}
}

