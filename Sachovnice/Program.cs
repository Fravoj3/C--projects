namespace Sachovnice;
using System;


class Ctecka{

    static char nextCharacter = ' ';
    private static bool firstMissing = true;

    static bool passedNl = false;

    private static char nl = Environment.NewLine[Environment.NewLine.Length - 1];

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
        if(nextCharacter == nl){
            passedNl = true;
        }
        return nextCharacter;
    }

    public static bool PassedNl(){
        return passedNl;
    }

    public static void resetNl(){
        passedNl = false;
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
            c = ReadChar();
            if (nextCharacter < '0' || nextCharacter > '9'){
                break;
            }
        }
        return negative? -outInt : outInt;
    }
    
    public static int[] decreaseByOne(int[] arr){
        int[] outArr = new int[arr.Length];
        for(int i = 0; i < arr.Length; i++){
            outArr[i] = arr[i] - 1;
        }
        return outArr;
    }
    public static void getToNextLine(){
        char c = ' ';
        while(nextCharacter != nl){
            c = ReadChar();
        }
    }
    public static int[] readIntsFromLine(int intCount){
        int[] outArr = new int[intCount];
        Ctecka.resetNl();
        for(int i = 0; i < intCount; i++){
            outArr[i] = readInt();
        }
        if(Ctecka.PassedNl()){
            throw new InvalidOperationException("Chyba při zpracování vstupu");
        }
        Ctecka.getToNextLine();
        return outArr;
    }


}
class Queue<T>{
    private T[] pole;
    private int zacatek;
    private int konec;
    private int kapacita;

    public Queue(int kapacita){
        this.kapacita = kapacita;
        pole = new T[kapacita];
        zacatek = 0;
        konec = 0;
    }

    public void add(T hodnota){
        if((konec + 1) % kapacita == zacatek){
            throw new InvalidOperationException("Fronta je plná");
        }
        pole[konec] = hodnota;
        konec = (konec + 1) % kapacita;
    }

    public T pop(){
        if(zacatek == konec){
            throw new InvalidOperationException("Fronta je prázdná");
        }
        T hodnota = pole[zacatek];
        zacatek = (zacatek + 1) % kapacita;
        return hodnota;
    }

    public bool isEmpty(){
        return zacatek == konec;
    }
}

class Ctecka2{
    public static char nextCharacter = ' ';
    private static bool firstMissing = true;

    public static void skipToNextLine(){
        char c = ' ';
        while(nextCharacter != '\n' || nextCharacter != '\r' || nextCharacter != '\0'){
            ReadChar();
        }
    }
    public static char ReadChar(){
        char charOut = ' ';
        if (firstMissing){
            charOut = (char)Console.Read();
            nextCharacter = (char)Console.Read();
            firstMissing = false;
            return charOut;
        }
        charOut = nextCharacter;
        nextCharacter = (char)Console.Read();
        return charOut;
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
            c = ReadChar();
            if (nextCharacter < '0' || nextCharacter > '9'){
                break;
            }
        }
        return negative? -outInt : outInt;
    }

    public static int[] readInts(int count, bool decreaseByOne=false){
        int[] outArr = new int[count];
        for(int i = 0; i < count; i++){
            if(decreaseByOne)
                outArr[i] = readInt() - 1;
            else
                outArr[i] = readInt();
        }
        return outArr;
    }

    public static int[] readIntsFromLine(int count, bool decreaseByOne=false){
        string[] line = Console.ReadLine().Split(' ');
        int[] outArr = new int[count];
        for(int i = 0; i < count; i++){
            if(decreaseByOne)
                outArr[i] = int.Parse(line[i]) - 1;
            else
                outArr[i] = int.Parse(line[i]);
        }
        return outArr;
    }
}


class Program
{

    static void Main(string[] args)
    {
        int pocetPrekazek = Ctecka2.readIntsFromLine(1)[0];
        int[] vychoziPozice = new int[2];
        int[] cilovaPozice = new int[2];

        // Vytvoř a naplň šachovnici  -1 - neprozkoumané pole, -2 - překážka, jinak délka od začátku
        int[][] sachovnice = new int[8][];
        for (int i = 0; i < 8; i++)
        {
            sachovnice[i] = new int[8];
            for(int j = 0; j < 8; j++)
            {
                sachovnice[i][j] = -1;
            }
        }

        // Načti překážky
        for(int i = 0; i < pocetPrekazek; i++){
            int[] pozicePrekazky = Ctecka2.readIntsFromLine(2, true);
            sachovnice[pozicePrekazky[0]][pozicePrekazky[1]] = -2;
        }

        vychoziPozice = Ctecka2.readIntsFromLine(2, true);
        cilovaPozice = Ctecka2.readIntsFromLine(2, true);

        // Vytvoř frontu
        Queue<int[]> fronta = new Queue<int[]>(65);

        // Přidej výchozí pozici do fronty
        fronta.add(vychoziPozice);
        sachovnice[vychoziPozice[0]][vychoziPozice[1]] = 0;

        while(!fronta.isEmpty()){
            int[] pozice = fronta.pop();
            int x = pozice[0];
            int y = pozice[1];
            int delka = sachovnice[pozice[0]][pozice[1]] + 1;

            if(pozice[0] == cilovaPozice[0] && pozice[1] == cilovaPozice[1]){
                System.Console.WriteLine(delka - 1);
                return;
            }
            // vlevo
            if(x > 0 && sachovnice[x-1][y] == -1){
                sachovnice[x-1][y] = delka;
                fronta.add(new int[]{x-1, y});
            }
            // nahoru
            if(y < 7 && sachovnice[x][y+1] == -1){
                sachovnice[x][y+1] = delka;
                fronta.add(new int[]{x, y+1});
            }
            // doprava
            if(x < 7 && sachovnice[x+1][y] == -1){
                sachovnice[x+1][y] = delka;
                fronta.add(new int[]{x+1, y});
            }
            // dolu
            if(y > 0 && sachovnice[x][y-1] == -1){
                sachovnice[x][y-1] = delka;
                fronta.add(new int[]{x, y-1});
            }

            // vlevo nahoru
            if (x > 0 && y < 7 && sachovnice[x-1][y+1] == -1){
                sachovnice[x-1][y+1] = delka;
                fronta.add(new int[]{x-1, y+1});
            }
            // vlevo dolu
            if (x > 0 && y > 0 && sachovnice[x-1][y-1] == -1){
                sachovnice[x-1][y-1] = delka;
                fronta.add(new int[]{x-1, y-1});
            }
            // doprava nahoru
            if (x < 7 && y < 7 && sachovnice[x+1][y+1] == -1){
                sachovnice[x+1][y+1] = delka;
                fronta.add(new int[]{x+1, y+1});
            }
            // doprava dolu
            if (x < 7 && y > 0 && sachovnice[x+1][y-1] == -1){
                sachovnice[x+1][y-1] = delka;
                fronta.add(new int[]{x+1, y-1});
            }
        }

        System.Console.WriteLine("-1");
        
    }

    
}
