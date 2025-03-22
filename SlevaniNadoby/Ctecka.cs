using System;

namespace SlevaniNadoby;
class Ctecka{

    static char nextCharacter = ' ';
    private static bool firstMissing = true;
    public static char ReadChar(){
        char currChar;
        if(firstMissing){
            currChar = (char)Console.Read();
            firstMissing = false;
            nextCharacter = (char)Console.Read();
        }else{
            currChar = nextCharacter;
            nextCharacter = (char)Console.Read();
        }
        return currChar;
    }

    public static char nextChar(){
        if(firstMissing){
            nextCharacter = (char)Console.Read();
            firstMissing = false;
        }
        return nextCharacter;
    }

    public static int readInt(){
        int outInt = 0;
        char c = ReadChar();
        bool negative = false;
        while(c < '0' || c > '9'){
            negative = false;
            c = ReadChar();
            if (c == '-'){
                negative = true;
                c = ReadChar();
            }
        }
        while(c >= '0' && c <= '9'){
            outInt = outInt * 10 + (c - '0');
            if (nextChar() < '0' || nextChar() > '9'){
                break;
            }
            c = ReadChar();
        }
        return negative? -outInt : outInt;
    }

}
