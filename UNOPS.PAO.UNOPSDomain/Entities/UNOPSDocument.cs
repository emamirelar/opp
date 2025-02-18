using UNOPS.PAO.Domain.Infrastructure;

namespace UNOPS.PAO.UNOPSDomain.Entities;

using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.UNOPSDomain.Entities.Common;

public class UNOPSDocument : Domain.Entities.Document
{
    public UNOPSDocument(bool linkedFile = false) : base()
    {
        this.LinkedFile = linkedFile;
    }
    public bool LinkedFile { get; set; }
}
