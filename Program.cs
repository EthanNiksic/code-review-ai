string url = args[0];
string[] parts = url.Split('/');

string owner = parts[3];
string repo = parts[4];
string number = parts[6];

string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/pulls/{number}";

Console.WriteLine("Owner:  " + owner);
Console.WriteLine("Repo:   " + repo);
Console.WriteLine("Number: " + number);
Console.WriteLine("API:    " + apiUrl);