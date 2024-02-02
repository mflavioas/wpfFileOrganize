using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

internal class Organizador
{
    // Lista de extensões de arquivo de imagem suportadas
    public static string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".heic" };

    private static DateTime GetImageDate(string filePath)
    {
        try
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Criar um decoder para a imagem
                BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);

                // Verificar se há metadados
                if (decoder.Metadata != null && decoder.Metadata is BitmapMetadata metadata)
                {
                    // Recuperar a data em que a imagem foi tirada
                    if (metadata.DateTaken != null)
                    {
                        return DateTime.Parse(metadata.DateTaken);
                    }
                }
            }
        }
        catch
        {
            return File.GetLastWriteTime(filePath);
        }
        return File.GetLastWriteTime(filePath);
    }

    private static string CalculateMD5Hash(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();

                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }

    private static List<string> GetFilesOrganize(string diretorioOrigem, List<string> extensoesArquivos)
    {
        // Obter todos os arquivos de imagem no diretório de origem e subdiretórios
        List<string> listaArquivosOrigens = new List<string>();
        foreach (string extension in extensoesArquivos)
        {
            string[] files = Directory.GetFiles(diretorioOrigem, "*" + (extension.Contains(".") ? "" : ".") + extension, SearchOption.AllDirectories);
            listaArquivosOrigens.AddRange(files);
        }
        return listaArquivosOrigens;
    }

    private static Dictionary<string, List<string>> GetListFileHashes(string diretorioOrigem, List<string> extensoesArquivos)
    {
        // Dicionário para armazenar o hash de cada arquivo
        Dictionary<string, List<string>> fileHashes = new Dictionary<string, List<string>>();

        List<string> listaArquivosOrigens = GetFilesOrganize(diretorioOrigem, extensoesArquivos);

        foreach (string filePath in listaArquivosOrigens)
        {
            string hash = CalculateMD5Hash(filePath);

            // Verificar se o hash já existe no dicionário
            if (fileHashes.ContainsKey(hash))
            {
                fileHashes[hash].Add(filePath);
            }
            else
            {
                // Adicionar o hash ao dicionário
                fileHashes.Add(hash, new List<string> { filePath });
            }
        }

        return fileHashes;
    }

    private const string C_IMAGENS = "TIF;TIFF;JPG;JPEG;BMP;GIF;HEIC;PNG";
    private const string C_VIDEOS = "MP4;AVI";

    public static Dictionary<string, List<string>> OrganizeImages(string sourceDirectory, string destinationDirectory, int tipoArquivos, bool moverArquivos, ref ProgressBar pb)
    {
        List<string> extensoesArquivos;
        if (tipoArquivos == 0)
            extensoesArquivos = C_IMAGENS.Split(';').ToList();
        else
            extensoesArquivos = C_VIDEOS.Split(';').ToList();
        
        Dictionary<string, List<string>> fileHashes = GetListFileHashes(sourceDirectory, extensoesArquivos);
        Dictionary<string, List<string>> Duplicados = new Dictionary<string, List<string>>();
        // Criar o diretório de destino, se não existir
        Directory.CreateDirectory(destinationDirectory);


        pb.Maximum = fileHashes.Count;
        pb.Minimum = 0;
        pb.Value = 0;

        // Mover os arquivos não duplicados para o diretório de destino
        foreach (var pair in fileHashes)
        {
            pb.Value++;
            int idxDuplicado = 0;
            foreach (string filePath in pair.Value)
            {
                DateTime imageDate = GetImageDate(filePath);
                string destinationPath = destinationDirectory;
                if (idxDuplicado > 0)
                    destinationPath = Path.Combine(destinationPath, "Duplicados", idxDuplicado.ToString());
                destinationPath = Path.Combine(destinationPath, imageDate.Year.ToString(), imageDate.ToString("MMMM"));
                Directory.CreateDirectory(destinationPath);

                string destinationFile = Path.Combine(destinationPath, Path.GetFileName(filePath));

                if (pair.Value.Count > 1)
                {
                    if (idxDuplicado == 0)
                        Duplicados.Add(pair.Key, new List<string> { destinationFile });
                    else
                        Duplicados[pair.Key].Add(destinationFile);
                }
                if (moverArquivos)
                {
                    File.Move(filePath, destinationFile);
                }
                else
                {
                    File.Copy(filePath, destinationFile);
                }
                idxDuplicado++;
            }
        }

        Console.WriteLine("Organização concluída!");
        return Duplicados;
    }
}
