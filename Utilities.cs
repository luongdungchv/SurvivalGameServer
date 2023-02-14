using System;

public class Utilities {
    public static string RandomSeed(){
        var randomObj = new Random(DateTime.Now.Millisecond);
        var res = randomObj.Next(1, 100000).ToString();
        //Console.WriteLine(DateTime.Now.Millisecond);
        return res;
    }
}