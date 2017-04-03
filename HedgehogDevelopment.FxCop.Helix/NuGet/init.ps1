param($installPath, $toolsPath, $package, $project)
[System.Reflection.Assembly]::LoadWithPartialName("System.Windows.Forms")

$DefaultRuleLocation = "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Team Tools\Static Analysis Tools\FxCop\Rules"
$ShowMessage = Test-Path ($DefaultRuleLocation + "\HedgehogDevelopment.FxCop.Helix.dll")

if ($ShowMessage -eq $False)
{
	[System.Windows.Forms.Messagebox]::Show("The file 'HedgehogDevelopment.FxCop.Helix.ddl' must be installed in the folder:`n`n" + $DefaultRuleLocation + "`n`nThis allows Visual Studio to edit the rules in a .ruleset.`nThis file can be found in folder the nuget package was installed in.", "Hedgehog Development", 'OK', 'Warning')
}