using System.Security.Cryptography.X509Certificates;

namespace Fhi.Slash.Public.SlashMessengerCLI.Tools;

/// <summary>
/// Tools for working with certificates.
/// </summary>
public static class CertificateTools
{
    /// <summary>
    /// Retrieves a certificate from the certificate store.
    /// </summary>
    /// <param name="onlyValid">If true, only valid certificates will be returned.</param>
    /// <param name="finds">The criteria to find the certificate (e.g., thumbprint, subject).</param>
    /// <returns>The found certificate as <see cref="X509Certificate2"/>.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if no matching certificate is found.</exception>
    public static X509Certificate2 GetFromStore(bool onlyValid = true, params (X509FindType FindType, object Value)[] finds)
    {
        ArgumentNullException.ThrowIfNull(finds);

        using X509Store x509Store = new(StoreName.My, StoreLocation.LocalMachine);
        x509Store.Open(OpenFlags.ReadOnly);

        X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates;
        for (int i = 0; i < finds.Length; i++)
        {
            (X509FindType FindType, object Value) = finds[i];
            X509FindType item = FindType;
            object item2 = Value;
            x509Certificate2Collection = x509Certificate2Collection.Find(item, item2, onlyValid);
        }

        X509Certificate2? x509Certificate = (from X509Certificate2 x in x509Certificate2Collection
                                             where x.NotBefore < DateTime.Now && x.NotAfter > DateTime.Now
                                             orderby x.NotBefore descending
                                             select x).FirstOrDefault();
        x509Store.Close();
        if (x509Certificate == null)
        {
            string value = string.Join("; ", finds.Select((f) => $"{f.FindType}={f.Value}"));
            throw new KeyNotFoundException($"No valid certfication was found for: {"onlyValid"}={onlyValid} and {value}");
        }

        return x509Certificate;
    }
}
