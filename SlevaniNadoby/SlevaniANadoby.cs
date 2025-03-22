using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace SlevaniNadoby;

class Nadoba{
    public int objem;
    public int plnost;
    public Nadoba(int objem, int plnost){
        this.objem = objem;
        this.plnost = plnost;
    }
}
class Stav{
    public Nadoba[] nadoby;
    public Stav(Nadoba[] nadoby){
        this.nadoby = nadoby;
    }
    public Stav(int a, int b, int c, int x, int y, int z){
        Nadoba[] nadoby = new Nadoba[3];
        nadoby[0] = new Nadoba(a, x);
        nadoby[1] = new Nadoba(b, y);
        nadoby[2] = new Nadoba(c, z);
        this.nadoby = nadoby;
    }

    public Stav(Stav pocatecniStav, string hash){
        int x = int.Parse(hash.Substring(0, 2));
        int y = int.Parse(hash.Substring(2, 2));
        int z = int.Parse(hash.Substring(4, 2));
        Nadoba[] nadoby = new Nadoba[3];
        nadoby[0] = new Nadoba(pocatecniStav.nadoby[0].objem, x);
        nadoby[1] = new Nadoba(pocatecniStav.nadoby[1].objem, y);
        nadoby[2] = new Nadoba(pocatecniStav.nadoby[2].objem, z);
        this.nadoby = nadoby;
    }    
    public void prelej(Nadoba z, Nadoba kam){
        int prelej = Math.Min(z.plnost, kam.objem - kam.plnost);
        z.plnost -= prelej;
        kam.plnost += prelej;
    }

    public Nadoba[] cloneNadoby(){
        Nadoba[] nadoby = new Nadoba[this.nadoby.Length];
        for(int i = 0; i < this.nadoby.Length; i++){
            nadoby[i] = new Nadoba(this.nadoby[i].objem, this.nadoby[i].plnost);
        }
        return nadoby;
    }
    public HashSet<String> generujStavy(){
        HashSet<String> stavy = new HashSet<String>();
        for(int i = 0; i < nadoby.Length; i++){
            for(int j = 0; j < nadoby.Length; j++){
                if(i != j){
                    Nadoba[] noveNadoby = cloneNadoby();
                    prelej(noveNadoby[i], noveNadoby[j]);
                    string hash = getHash(noveNadoby);
                    if(!stavy.Contains(hash)){
                        stavy.Add(hash);
                    }
                }
            }
        }
        return stavy;
    }
     public string getHash(){
        string hash = "";
        for(int i = 0; i < nadoby.Length; i++){
            Nadoba n = nadoby[i];
            if (n.plnost == 10){
                hash += n.plnost;
            }else{
                hash += "0" + n.plnost;
            }
        }
        return hash;
     }
     public string getHash(Nadoba[] nadoby){
        string hash = "";
        for(int i = 0; i < nadoby.Length; i++){
            Nadoba n = nadoby[i];
            if (n.plnost == 10){
                hash += n.plnost;
            }else{
                hash += "0" + n.plnost;
            }
        }
        return hash;
     }
}
class Stavy{
    public HashSet<string> stavy;
    public Hashtable mozneObjemy;

    public Stavy(){
        stavy = new HashSet<string>();
        mozneObjemy = new Hashtable();
    }

    public void VygenerujMozneStavy(Stav pocatecniStav){
        stavy = new HashSet<string>();
        mozneObjemy = new Hashtable();
        Queue<string> stavyKProzkoumani = new Queue<string>();
        stavyKProzkoumani.Enqueue(pocatecniStav.getHash()+"0");
        stavy.Add(pocatecniStav.getHash());
        while(stavyKProzkoumani.Count > 0){
            string stavHashFull = stavyKProzkoumani.Dequeue();
            string stavHash = stavHashFull.Substring(0, 6);
            int pocetKroku = int.Parse(stavHashFull.Substring(6));

            Stav stav = new Stav(pocatecniStav, stavHash);
            // Pridej objemy
            for(int i = 0; i < stav.nadoby.Length; i++){
                if(!mozneObjemy.ContainsKey(stav.nadoby[i].plnost)){
                    mozneObjemy.Add(stav.nadoby[i].plnost, pocetKroku);
                }
            }
            // Vygeneruj nove stavy
            HashSet<string> noveStavy = stav.generujStavy();
            foreach(string s in noveStavy){
                if(stavy.Contains(s)){
                    continue;
                }else{
                    stavy.Add(s);
                    stavyKProzkoumani.Enqueue(s+(pocetKroku+1));
                }
            }
        }
        
        
    }

    public void VytiskniMozneObjemy(){
        var sortedEntries = mozneObjemy.Cast<DictionaryEntry>()
                                       .OrderBy(entry => entry.Key)
                                       .ToList();
        bool first = true;
        string outStr = "";
        foreach (var entry in sortedEntries){
            if(!first){
                //Console.Write(" ");
                outStr += " ";
            }else{
                first = false;
            }
            //Console.Write($"{entry.Key}:{entry.Value}");
            outStr += $"{entry.Key}:{entry.Value}";
        }
        Console.WriteLine(outStr);
    }
}
