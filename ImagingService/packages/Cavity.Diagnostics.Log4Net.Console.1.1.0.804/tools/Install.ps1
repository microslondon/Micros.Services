param($installPath, $toolsPath, $package, $project)

$project.ProjectItems.Item("log4net.config").Properties.Item("CopyToOutputDirectory").Value = 1

$item = $project.ProjectItems.Item("Program.cs")

$terminator = ""
if ($item.FileCodeModel.Language -eq "{B5E9BD34-6D3E-4B5D-925E-8A43B79820B4}")
{
    $terminator = ";"
}

$win = $item.Open("{7651A701-06E5-11D1-8EBD-00A0C90F26EA}")
$text = $win.Document.Object("TextDocument");
$namespace = $item.FileCodeModel.CodeElements | where-object {$_.Kind -eq 5}

$class = $namespace.Children | where-object {$_.Kind -eq 1}

$methods = $class.Children | where-object {$_.Name -eq "Main"}

$edit = $methods.StartPoint.CreateEditPoint();
$edit.LineDown()
$edit.CharRight(1)
$edit.Insert([Environment]::NewLine)
$edit.Insert("            log4net.Config.XmlConfigurator.Configure()")
$edit.Insert($terminator)