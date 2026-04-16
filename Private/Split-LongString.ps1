Function Split-LongString
    {
        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$true, ValueFromPipeline=$True)]
            [string]$Text,
            [Parameter(Mandatory=$False)]
            [int]$Width = [Console]::WindowWidth,
            [Parameter(Mandatory=$False)]
            [int]$Padding = 4
        )

        # helper to strip ANSI/escape sequences so length comparisons use printable characters
        function Remove-AnsiCodes
            {
                param(
                    [string]$t
                )
                If(-not $t)
                    {
                        return ''
                    }
                $regex = "{0}\[[0-9;]*m" -f [ConsoleFX.Ansi]::Escape
                return $t -replace $regex, ''
            }
        function Get-PrintableLength
            {
                param(
                    [string]$t
                )
                return (Remove-AnsiCodes -t $t).Length
            }

        $Width = $Width - $Padding
        $Lines = @()

        # Split by newlines first to preserve line structure
        $InputLines = $Text -split "`n"

        ForEach($InputLine in $InputLines)
            {
                # If line is short enough, keep it as-is
                If((Get-PrintableLength $InputLine) -le $Width)
                    {
                        $Lines += $InputLine
                    }
                Else
                    {
                        # Only wrap lines that are too long
                        $Words = $InputLine -split '\s+'
                        $Line = ''
                        $LineLen = 0

                        ForEach($Word in $Words)
                            {
                                $wordLen = Get-PrintableLength $Word
                                $spaceLen = if ($LineLen -gt 0) {1} else {0}
                                if(($LineLen + $wordLen + $spaceLen) -le $Width)
                                    {
                                        if($LineLen -gt 0) {
                                            $Line += ' '
                                            $LineLen += 1
                                        }
                                        $Line += $Word
                                        $LineLen += $wordLen
                                    }
                                Else
                                    {
                                        $Lines += $Line
                                        $Line = $Word
                                        $LineLen = $wordLen
                                    }
                            }

                        If($Line)
                            {
                                $Lines += $Line
                            }
                    }
            }

        Return $Lines
    }