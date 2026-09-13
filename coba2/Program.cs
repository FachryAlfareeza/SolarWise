using System;

class Program
{
    static void Main(string[] args)
    {
        int[] angka = {67, 911, 69, 711, 17, 21, 15, 33};
        TampilkanKumpulanAngka(angka);
        int[] angkaUrut = BubbleSort(angka);
        TampilkanKumpulanAngka(angkaUrut);
        Console.ReadLine();
    }

    private static void TampilkanKumpulanAngka(int[] angka)
    {
        foreach (int a in angka)
        {
            Console.WriteLine($"{a} ");
        }
        Console.WriteLine();
    }

    private static void TukarTempatAngka(ref int angka1, ref int angka2)
    {
        int temp = angka1;
        angka1 = angka2;
        angka2 = temp;
    }

    private static int[] BubbleSort(int[] angka)
    {
        bool terurut = false;
        while (!terurut)
        {
            terurut = true;
            for (int i = 0; i < angka.Length - 1; i++)
            {
                if (angka[i] > angka[i + 1])
                {
                    TukarTempatAngka(ref angka[i], ref angka[i + 1]);
                    terurut = false;
                }
            }
        }
        return angka;
    }
}