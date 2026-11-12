using System.Globalization;
using System.Text;

namespace PetrobrasFInin.Services;

// Gera um PDF textual pequeno sem biblioteca externa.
// Os relatórios completos podem ser visualizados na interface e impressos pelo navegador.
public static class PdfSimples
{
    private static string Ascii(string texto)
    {
        var normalizado = (texto ?? "").Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (c == '\r' || c == '\n') { sb.Append(' '); continue; }
            sb.Append(c is >= ' ' and <= '~' ? c : '?');
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
    private static string Escape(string texto) => Ascii(texto)
        .Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    public static byte[] Gerar(IEnumerable<string> linhas)
    {
        var entrada = linhas.ToList();
        if (entrada.Count == 0) entrada.Add("SIAF - Sem registros");
        var paginas = entrada.Chunk(48).ToArray();
        var objetos = new List<byte[]>();
        int Add(string objeto) { objetos.Add(Encoding.Latin1.GetBytes(objeto)); return objetos.Count; }
        int catalogo = Add("");
        int arvore = Add("");
        int fonte = Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>");
        var paginasIds = new List<int>();
        foreach (var pagina in paginas)
        {
            var texto = new StringBuilder("BT /F1 10 Tf 45 790 Td 14 TL\n");
            foreach (var linha in pagina)
                texto.Append('(').Append(Escape(linha.Length > 100 ? linha[..100] : linha)).Append(") Tj T*\n");
            texto.Append("ET\n");
            var bytesTexto = Encoding.Latin1.GetBytes(texto.ToString());
            int streamId = Add($"<< /Length {bytesTexto.Length} >>\nstream\n{texto}endstream");
            int pageId = Add($"<< /Type /Page /Parent {arvore} 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {fonte} 0 R >> >> /Contents {streamId} 0 R >>");
            paginasIds.Add(pageId);
        }
        objetos[arvore - 1] = Encoding.Latin1.GetBytes($"<< /Type /Pages /Kids [{string.Join(" ", paginasIds.Select(x => $"{x} 0 R"))}] /Count {paginasIds.Count} >>");
        objetos[catalogo - 1] = Encoding.Latin1.GetBytes($"<< /Type /Catalog /Pages {arvore} 0 R >>");
        using var ms = new MemoryStream();
        void Put(string s) { var b = Encoding.Latin1.GetBytes(s); ms.Write(b, 0, b.Length); }
        Put("%PDF-1.4\n");
        var offsets = new List<long> { 0 };
        for (int i = 0; i < objetos.Count; i++)
        {
            offsets.Add(ms.Position);
            Put($"{i + 1} 0 obj\n");
            ms.Write(objetos[i]);
            Put("\nendobj\n");
        }
        long xref = ms.Position;
        Put($"xref\n0 {objetos.Count + 1}\n0000000000 65535 f \n");
        foreach (long o in offsets.Skip(1)) Put($"{o:0000000000} 00000 n \n");
        Put($"trailer\n<< /Size {objetos.Count + 1} /Root {catalogo} 0 R >>\nstartxref\n{xref}\n%%EOF\n");
        return ms.ToArray();
    }
}
