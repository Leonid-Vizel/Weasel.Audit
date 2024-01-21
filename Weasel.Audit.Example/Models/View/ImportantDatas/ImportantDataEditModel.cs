using Weasel.Audit.Example.Models.Data.ImportantDatas;

namespace Weasel.Audit.Example.Models.View.ImportantDatas;

public sealed class ImportantDataEditModel : ImportantDataBase
{
    public int Id { get; set; }
    public ImportantDataEditModel(ImportantData model) : base(model)
    {
        Id = model.Id;
    }
}
