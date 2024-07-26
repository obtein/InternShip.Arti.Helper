using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Text;


byte resultGlobal = 0x00;



string ByteToString ( byte x )
{
    return x.ToString( "X2" );
}

int StringToInt (string x) 
{
    return int.Parse( x, System.Globalization.NumberStyles.HexNumber );
}

byte[] SplitByteToTwo (byte x)
{
    return Encoding.ASCII.GetBytes( ByteToString(x) );
}

int HexToInt ( char hexChar )
{
    hexChar = char.ToUpper( hexChar );
    return (int)hexChar < (int)'A' ?
        ( (int)hexChar - (int)'0' ) :
        10 + ( (int)hexChar - (int)'A' );
}

byte MSB_TwoByteASCIIToByte ( byte msb_ascii, byte lsb_ascii )
{
    int MSB = HexToInt( (char)msb_ascii );
    int LSB = HexToInt( (char)lsb_ascii );
    return (byte)( ( MSB * 16 ) + LSB );
}

void CalculateSplittedByte()
{
    Console.WriteLine("Enter the value");
    string input = Console.ReadLine();
    byte x = (byte)StringToInt(input);
    int intX = StringToInt( ByteToString( x ) );

    Console.WriteLine( "HEX TO BE SPLIT <0x" + ByteToString( x ) + ">  |||| <" + x + ">" );
    byte [] xValues = SplitByteToTwo( x );
    byte msb = xValues [0];
    byte lsb = xValues [1];
    byte resultTest = MSB_TwoByteASCIIToByte( msb, lsb );
    string resultString = ByteToString( resultTest );

    Console.WriteLine( "MSB: " + Convert.ToChar( msb ) );
    Console.WriteLine( "LSB: " + Convert.ToChar( lsb ) );
    Console.WriteLine( "RESULT : " + resultTest );

    string msbHex = msb.ToString( "X2" );
    string lsbHex = lsb.ToString( "X2" );
    Console.WriteLine( "hex msb: 0x" + msbHex );
    Console.WriteLine( "hex lsb: 0x" + lsbHex );
    Console.WriteLine( "hex full: 0x" + resultString );
    Console.ReadLine();
}

void CalculateSplittedByteWithInput (byte x)
{
    Console.WriteLine( "HEX TO BE SPLIT <0x" + ByteToString( x ) + ">  |||| <" + x + ">" );
    byte [] xValues = SplitByteToTwo( x );
    byte msb = xValues [0];
    byte lsb = xValues [1];
    byte resultTest = MSB_TwoByteASCIIToByte( msb, lsb );
    string resultString = ByteToString( resultTest );

    Console.WriteLine( "MSB: " + Convert.ToChar( msb ) );
    Console.WriteLine( "LSB: " + Convert.ToChar( lsb ) );
    Console.WriteLine( "RESULT : " + resultTest );

    string msbHex = msb.ToString( "X2" );
    string lsbHex = lsb.ToString( "X2" );
    Console.WriteLine( "hex msb: 0x" + msbHex );
    Console.WriteLine( "hex lsb: 0x" + lsbHex );
    Console.WriteLine( "hex full: 0x" + resultString );
}

void CalculateCheckSum () 
{
    int count = 0;
    string input;
    byte result = 0x00;
    Console.WriteLine("Press - to exit, + to calculate splitted version of the result");
    do
    {
        Console.WriteLine("Enter " + (count++) + ". value");
        var written = Console.ReadLine();
        if (written != "-" && written != "+")
        {
            byte x = (byte)StringToInt( written );
            result ^= x;
            Console.WriteLine( "Result so far : 0x" + ByteToString( result ) );
        }
        input = written;
    } while ( input != "-" && input != "+" );
    if ( input == "+" )
    {
        resultGlobal = result;
        CalculateSplittedByteWithInput( resultGlobal );
        Console.ReadLine();
    }
}

byte[] CalculateCheckSumWithByteArray ( byte [] x)
{
    byte [] result = new byte[2];
    byte temp = 0x00;
    for ( int i = 0; i < x.Length; i++)
    {
        temp ^= x [i];
    }
    byte [] xValues = SplitByteToTwo( temp );
    result [0] = xValues [0]; // msb
    result [1] = xValues [1]; // lsb
    return result;
}

byte [] CalculateBytesToSendOpenChannel ( int cardIndex, int channelIndex )
{
    byte toBegin = 0b00000001;
    byte [] result = new byte[12]; // to be returned
    byte [] checkSumCalc = new byte [9]; // to calculate checksum
    byte [] checkSumTemp = new byte [2]; // to send checksum Method
    // Generic properties
    byte stx = 0x02;
    byte etx = 0x03;
    byte id = 0x39;
    byte msgCode = 0x00;
    // Data part
    byte dataLengthMSB = 0x30;
    byte dataLengthLSB = 0x34;
    byte dataByte1MSB = 0xFF;
    byte dataByte1LSB = 0x00;
    byte dataByte0MSB = 0xFF;
    byte dataByte0LSB = 0x00;
    // Check sum
    byte checkSumMSB;
    byte checkSumLSB;

    if ( cardIndex == 0 )
    {
        msgCode = 0xD7;
    }
    else if ( cardIndex == 1 )
    {
        msgCode = 0xD8;
    }
    else if ( cardIndex == 2 )
    {
        msgCode = 0xD9;
    }

    if ( channelIndex == 1 )
    {
        byte temp1 = toBegin;
        byte temp0 = (byte)(~temp1);
        byte [] split1 = SplitByteToTwo( temp1 );
        byte [] split0 = SplitByteToTwo( temp0 );
        dataByte1MSB = split1 [0];
        dataByte1LSB = split1 [1];
        dataByte0MSB = split0 [0];
        dataByte0LSB = split0 [1];
    }
    else if ( channelIndex == 2 )
    {
        byte temp1 = (byte)(toBegin << 1);
        byte temp0 = (byte)( ~temp1 );
        byte [] split1 = SplitByteToTwo( temp1 );
        byte [] split0 = SplitByteToTwo( temp0 );
        dataByte1MSB = split1 [0];
        dataByte1LSB = split1 [1];
        dataByte0MSB = split0 [0];
        dataByte0LSB = split0 [1];
    }
    else if ( channelIndex == 3 )
    {
        byte temp1 = (byte)( toBegin << 2 );
        byte temp0 = (byte)( ~temp1 );
        byte [] split1 = SplitByteToTwo( temp1 );
        byte [] split0 = SplitByteToTwo( temp0 );
        dataByte1MSB = split1 [0];
        dataByte1LSB = split1 [1];
        dataByte0MSB = split0 [0];
        dataByte0LSB = split0 [1];
    }
    else if ( channelIndex == 4 )
    {
        byte temp1 = (byte)( toBegin << 3 );
        byte temp0 = (byte)( ~temp1 );
        byte [] split1 = SplitByteToTwo( temp1 );
        byte [] split0 = SplitByteToTwo( temp0 );
        dataByte1MSB = split1 [0];
        dataByte1LSB = split1 [1];
        dataByte0MSB = split0 [0];
        dataByte0LSB = split0 [1];
    }
    else if ( channelIndex == 5 )
    {
        byte temp1 = (byte)( toBegin << 4 );
        byte temp0 = (byte)( ~temp1 );
        byte [] split1 = SplitByteToTwo( temp1 );
        byte [] split0 = SplitByteToTwo( temp0 );
        dataByte1MSB = split1 [0];
        dataByte1LSB = split1 [1];
        dataByte0MSB = split0 [0];
        dataByte0LSB = split0 [1];
    }
    else if ( channelIndex == 6 )
    {
        byte temp1 = (byte)( toBegin << 5 );
        byte temp0 = (byte)( ~temp1 );
        byte [] split1 = SplitByteToTwo( temp1 );
        byte [] split0 = SplitByteToTwo( temp0 );
        dataByte1MSB = split1 [0];
        dataByte1LSB = split1 [1];
        dataByte0MSB = split0 [0];
        dataByte0LSB = split0 [1];
    }
    else if ( channelIndex == 7 )
    {
        byte temp1 = (byte)( toBegin << 6 );
        byte temp0 = (byte)( ~temp1 );
        byte [] split1 = SplitByteToTwo( temp1 );
        byte [] split0 = SplitByteToTwo( temp0 );
        dataByte1MSB = split1 [0];
        dataByte1LSB = split1 [1];
        dataByte0MSB = split0 [0];
        dataByte0LSB = split0 [1];
    }
    else if ( channelIndex == 8 )
    {
        byte temp1 = (byte)( toBegin << 7 );
        byte temp0 = (byte)( ~temp1 );
        byte [] split1 = SplitByteToTwo( temp1 );
        byte [] split0 = SplitByteToTwo( temp0 );
        dataByte1MSB = split1 [0];
        dataByte1LSB = split1 [1];
        dataByte0MSB = split0 [0];
        dataByte0LSB = split0 [1];
    }

    checkSumCalc [0] = stx;
    checkSumCalc [1] = id;
    checkSumCalc [2] = msgCode;
    checkSumCalc [3] = dataLengthMSB;
    checkSumCalc [4] = dataLengthLSB;
    checkSumCalc [5] = dataByte1MSB;
    checkSumCalc [6] = dataByte1LSB;
    checkSumCalc [7] = dataByte0MSB;
    checkSumCalc [8] = dataByte0LSB;
    checkSumTemp = CalculateCheckSumWithByteArray(checkSumCalc);
    checkSumMSB = checkSumTemp [0];
    checkSumLSB = checkSumTemp [1];

    result [0] = stx;
    result [1] = id;
    result [2] = msgCode;
    result [3] = dataLengthMSB;
    result [4] = dataLengthLSB;
    result [5] = dataByte1MSB;
    result [6] = dataByte1LSB;
    result [7] = dataByte0MSB;
    result [8] = dataByte0LSB;
    result [9] = checkSumMSB;
    result [10] = checkSumLSB;
    result [11] = etx;
    return result;
}

void CalculateBytesToSendOpenChannelMenu () 
{
    int input = 0;
    int cardIndex;
    int channelIndex;
    byte [] resultToBePrinted;
    do
    {
        Console.WriteLine("If you want to continue pls enter a positive number ");
        input = Convert.ToInt32(Console.ReadLine());
        if ( input >= 0 )
        {
            Console.WriteLine("Please enter card Index");
            cardIndex = Convert.ToInt32( Console.ReadLine() );
            Console.WriteLine("Please enter channel Index for " + cardIndex + ". card");
            channelIndex = Convert.ToInt32( Console.ReadLine() );
            resultToBePrinted = CalculateBytesToSendOpenChannel(cardIndex,channelIndex);
            for ( int i = 0; i < resultToBePrinted.Length; i++ )
            {
                if ( i == 0 )
                    Console.Write("{ ");
                Console.Write( "0x" + ByteToString( resultToBePrinted [i] ) );
                if ( i == 11 )
                    Console.Write("}");
                if( i != 11)
                    Console.Write(", ");
            }
        }
        Console.WriteLine();
    } while ( input >= 0 );
}

bool flag = true;
do
{
    Console.WriteLine("<<<<<<<<<<<<<<<<<MENU>>>>>>>>>>>>>>");
    Console.WriteLine( "1- Split Byte" );
    Console.WriteLine( "2- Calculate checksum" );
    Console.WriteLine( "3- CalculateBytesToSendOpenChannel" );
    Console.WriteLine( "0- Exit" );
    Console.WriteLine( "<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>" );

    Console.WriteLine("Enter your choice...");
    int userChoice = Convert.ToInt32(Console.ReadLine());
    switch ( userChoice )
    {
        case 0:
            flag = false;
            break;
        case 1:
            CalculateSplittedByte();
            break;
        case 2:
            CalculateCheckSum();
            break;
        case 3:
            CalculateBytesToSendOpenChannelMenu();
            break;
        default:
            // code block
            break;
    }
    Console.Clear();
    Console.WriteLine( "\x1b[3J" );
} while ( flag );



