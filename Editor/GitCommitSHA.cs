#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public static class GitCommitSHA
{
    private static string ProjectGitFolder => Path.Combine(Directory.GetCurrentDirectory(), ".git");
    
    public static string Value
    {
        get
        {
            string headFilePath = Path.Combine(ProjectGitFolder, "HEAD");

            if (!File.Exists(headFilePath))
                return "";

            string headFileContent = File.ReadAllText(headFilePath);
            return GetShaFromHeadFile(headFileContent);
        }
    }

    private static string GetShaFromHeadFile(string headFileContent)
    {
        string gitCommitSHA = String.Empty;
        
        var match = Regex.Match(headFileContent, @"^.+(refs/heads/.+)$");
        if (match.Success)
        {
            var referenceFilePath = Path.Combine(ProjectGitFolder, match.Groups[1].Value);
            if (File.Exists(referenceFilePath))
            {
                // if ref are not packed.
                gitCommitSHA = File.ReadAllText(referenceFilePath).Substring(0, 7);
            }
            else
            {
                // if ref are packed.
                referenceFilePath = Path.Combine(ProjectGitFolder, "packed-refs");
                var packedGuids = File.ReadAllText(referenceFilePath);
                var matchingLine = Regex.Match(packedGuids, $"^([0-9a-zA-Z]+).*({match.Groups[1].Value}).*$", RegexOptions.Multiline);
                if(matchingLine.Success)
                {
                    gitCommitSHA = matchingLine.Groups[0].Value.Substring(0, 7);
                }
                else
                {
                    Debug.LogWarning($"BuildProcess: Get SHA failed, packed git refs file content: {packedGuids}");
                }
            }
        }
        else if (Regex.Match(headFileContent, @"^[0-9a-zA-Z]+$").Success)
        {
            gitCommitSHA = headFileContent.Substring(0, 7);
        }
        else
        {
            Debug.LogWarning($"BuildProcess: Get SHA failed, git HEAD file content: {headFileContent}");
        }
        
        return gitCommitSHA;
    }
}
#endif