using System.CommandLine;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

var bundleCommand = new Command("bundle", "bundle files into one file");
var rootCommand = new RootCommand();

var outputOp = new Option<FileInfo>("--output");
var languageOp = new Option<string[]>("--language") { IsRequired= true,AllowMultipleArgumentsPerToken=true };
var noteOp=new Option<bool>("--note");
var sortOp = new Option<bool>("--sort");
var removeEmptyLinesOp=new Option<bool>("--remove-lines");
var authorOp=new Option<string>("--ahuthor");
outputOp.AddAlias("-o");
languageOp.AddAlias("-l");
noteOp.AddAlias("-n");
sortOp.AddAlias("-s");
removeEmptyLinesOp.AddAlias("-rl");
authorOp.AddAlias("-a");

bundleCommand.AddOption(outputOp);
bundleCommand.AddOption(languageOp);
bundleCommand.AddOption(noteOp);
bundleCommand.AddOption(sortOp);
bundleCommand.AddOption(removeEmptyLinesOp);
bundleCommand.AddOption(authorOp);


bundleCommand.SetHandler((output,lang,note,sort,remove,author) =>
   {

       string path;
       string myDirectory=Directory.GetCurrentDirectory();
       try
       {
           string[] finals = languages(lang);
           if (finals[0].Contains(' '))
               throw new Exception(finals[0]);
           if (output != null)
               if (!string.IsNullOrEmpty(output.DirectoryName))
                   path = output.FullName;
               else path = myDirectory + "\\" + output.FullName;
           else path = myDirectory + "\\bundle";
           var filesList = Directory.GetFiles(myDirectory, "*.*", SearchOption.AllDirectories).ToList().FindAll(f=>finals.Contains(Path.GetExtension(f)));
           File.WriteAllText(path,"");
           if (author != null)
           {
               File.AppendAllText(path, "------- " + author + " ------- \n");
           }
               if (sort)
                   filesList = [.. filesList.OrderBy(f => Path.GetExtension(f)).ThenBy(f => Path.GetFileName(f))];
               else
                   filesList = [.. filesList.OrderBy(f => Path.GetFileName(f))];
               filesList.ForEach(file =>
               {
                   File.AppendAllText(path, "\n");
                   if (note)
                   {
                       File.AppendAllText(path, "------- "+file + " ------- \n");
                       
                   }
                   var lines = File.ReadAllLines(file).ToList();
                   lines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                   File.WriteAllLines(file, lines);
                   File.AppendAllText(path, File.ReadAllText(file));
               });
       }
       catch (Exception e)
       {
           Console.WriteLine( e.Message);
       }    
    

}
,outputOp,languageOp, noteOp,sortOp,removeEmptyLinesOp,authorOp);

var createRspCommand = new Command("create-rsp");
createRspCommand.AddAlias("cr");
createRspCommand.SetHandler(() =>
{

StreamWriter sw = new(Path.Combine(Directory.GetCurrentDirectory(), "bundle.rsp"));
var input = "";
sw.Write("bundle");
Console.WriteLine("output?");
input = Console.ReadLine();
if (!string.IsNullOrEmpty(input))
    sw.Write(" -o " + input);
Console.WriteLine("language?");
input = Console.ReadLine();
while (string.IsNullOrEmpty(input))
{
    Console.WriteLine("language is required");
    input = Console.ReadLine();
}
if (languages(input.Split(' '))[0].Contains(' '))
    throw new Exception(input.Split(' ')[0]);
        sw.Write(" -l " + input);
        Console.WriteLine("sort by type? y/n (default : sort by name)");
    try
    {
        input = Console.ReadLine();
        if (input == "y")
            sw.Write(" -s ");
        else if (input != "n")
            throw new Exception("ERROR: invalid value");
        Console.WriteLine("note? y/n");
        input = Console.ReadLine();
        if (input == "y")
            sw.Write(" -n ");
        else if (input != "n")
            throw new Exception("ERROR: invalid value");
        Console.WriteLine("remove-empty-lines? y/n");
        input = Console.ReadLine();
        if (input == "y")
            sw.Write(" -rl ");
        else if (input != "n")
            throw new Exception("ERROR: invalid value");
        Console.WriteLine("author?");
        input = Console.ReadLine();
        if (!string.IsNullOrEmpty(input))
            sw.Write(" -a" + input);
        Console.WriteLine("run 'file @bundle.rsp' to bundle the files");

    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }

     sw.Close(); 
});
string[] languages(string[] langs){
    string langs2 = "";
    foreach (var item in langs)
    {
        var langs3 = languages2(item);
        if (langs3.Contains(' '))
            return [langs3];
        langs2+=langs3+",";
    }
    return langs2.Split(',');
}

 string languages2(string lang)
{
    return lang switch
    {
        "c#" => ".cs,",
        "c" => ".cpp,.h,",
        "c++" => "",
        "java" => ".java,.class,.jar,",
        "python" => ".py,.pyw,",
        "react" => ".tsx,.html,.js,.ts,.jsx,",
        "angular" => ".ts,.html,.css,",
        "javaScript" => ".js,",
        "html" => ".html,",
        "all" =>".txt,"+ languages2("c#") +  languages2("c")  + languages2("c++") + languages2("java") + languages2("python") + languages2("react") + languages2("angular") + languages2("javaScript") + languages2("html"),
        _ => "ERROR: " + lang + " is not a known argument check if you wrote the word correctly"

    };
 }
  rootCommand.AddCommand(bundleCommand);
  rootCommand.AddCommand(createRspCommand);
  await rootCommand.InvokeAsync(args);