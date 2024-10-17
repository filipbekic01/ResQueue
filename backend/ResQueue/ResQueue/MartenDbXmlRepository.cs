using Microsoft.AspNetCore.DataProtection.Repositories;
using System.Xml.Linq;
using Marten;

namespace ResQueue;

public class MartenDbXmlRepository(
    IDocumentSession _documentSession
) : IXmlRepository
{
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        var keys = _documentSession.Query<DataProtectionKey>().ToList();

        var elements = keys.Select(k => XElement.Parse(k.XmlData)).ToList().AsReadOnly();

        return elements;
    }

    public void StoreElement(XElement element, string friendlyName)
    {
        var key = new DataProtectionKey
        {
            FriendlyName = friendlyName,
            XmlData = element.ToString(SaveOptions.DisableFormatting)
        };

        _documentSession.Store(key);

        _documentSession.SaveChanges();
    }
}