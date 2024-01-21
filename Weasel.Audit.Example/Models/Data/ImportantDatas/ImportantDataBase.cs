namespace Weasel.Audit.Example.Models.Data.ImportantDatas;

public abstract class ImportantDataBase
{
    public string StringData { get; set; } = null!;
    public DateTime DateTimeData { get; set; }
    public int IntegerDate { get; set; }
    public ImportantDataBase() : base() { }
    public ImportantDataBase(ImportantDataBase model) : base()
    {
        StringData = model.StringData;
        DateTimeData = model.DateTimeData;
        IntegerDate = model.IntegerDate;
    }
}