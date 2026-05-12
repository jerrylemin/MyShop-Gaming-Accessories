param(
    [Parameter(Mandatory = $true)]
    [string]$AppPath,
    [string]$Username = "admin",
    [string]$Password = "MyShop123!"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms

function Find-ElementByName {
    param(
        [System.Windows.Automation.AutomationElement]$Root,
        [string]$Name,
        [int]$TimeoutSeconds = 10
    )

    $condition = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty,
        $Name)
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    do {
        $element = $Root.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
        if ($null -ne $element) {
            return $element
        }

        Start-Sleep -Milliseconds 250
    } while ((Get-Date) -lt $deadline)

    throw "Could not find UI element named '$Name'."
}

function Invoke-Element {
    param([System.Windows.Automation.AutomationElement]$Element)
    $pattern = $Element.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern)
    $pattern.Invoke()
}

$process = Start-Process -FilePath $AppPath -PassThru
try {
    Start-Sleep -Seconds 3
    $root = [System.Windows.Automation.AutomationElement]::RootElement
    $loginWindow = Find-ElementByName -Root $root -Name "MyShop Gaming Accessories" -TimeoutSeconds 20

    $usernameBox = Find-ElementByName -Root $loginWindow -Name "Enter your username"
    $passwordBox = Find-ElementByName -Root $loginWindow -Name "Password"
    $usernameBox.SetFocus()
    [System.Windows.Forms.SendKeys]::SendWait("^a")
    [System.Windows.Forms.SendKeys]::SendWait($Username)
    $passwordBox.SetFocus()
    [System.Windows.Forms.SendKeys]::SendWait($Password)
    Invoke-Element (Find-ElementByName -Root $loginWindow -Name "Sign in")

    Start-Sleep -Seconds 5
    $mainWindow = Find-ElementByName -Root $root -Name "MyShop Gaming Accessories POS" -TimeoutSeconds 20
    foreach ($target in @("Products", "Orders", "Reports", "Settings")) {
        Invoke-Element (Find-ElementByName -Root $mainWindow -Name $target)
        Start-Sleep -Seconds 1
    }

    Write-Host "UI smoke test passed: login and main navigation opened Products, Orders, Reports, and Settings."
}
finally {
    if (!$process.HasExited) {
        $process.CloseMainWindow() | Out-Null
    }
}
