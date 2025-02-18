namespace UNOPS.PAO.Utilities.Interfaces;

using UNOPS.PAO.Domain.Infrastructure;

public interface IEmailSenderCoordinator
{
    public void SendErrorLog(ErrorLog errorLog);
}