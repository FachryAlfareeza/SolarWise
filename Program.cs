try
{
    Console.WriteLine("infoin angka bang!");
    int number = int.Parse(Console.ReadLine() ?? "0");
    Console.WriteLine("lu tadi masukin angka " + number.ToString());
    Console.ReadLine();
}
catch (Exception e)
{
    Console.WriteLine("lu salah masukin angka bang! jadinya error: " + e.Message);
}
Console.ReadLine();
    