Function Show-LayeredGridMenu
    {
        <#
        .SYNOPSIS
            Displays a stack of grid menus based on a list of menu definition objects.
            Menus are rendered in ascending order by Index when provided; otherwise original list order is used.
            Each completed menu is redrawn dimmed in the background while the next menu is shown over the top.
            Pressing Escape on a menu goes back one menu (if available).

        .EXAMPLE
            $MenuItems = @(
                @{
                    Index = 0
                    Title = "FirstMenu"
                    Items = @('Choice 1', 'Choice 2', 'Choice 3')
                    StartRow = 0
                    BorderInset = 0
                    Border = $true
                },
                @{
                    Index = 1
                    Title = "SecondMenu"
                    Items = @('Choice 1', 'Choice 2', 'Choice 3')
                    StartRow = 2
                    BorderInset = 4
                    Border = $true
                }
            )

            Show-LayeredGridMenu -MenuItems $MenuItems
        #>

        [CmdletBinding()]
        param(
            [Parameter(Mandatory=$True, Position = 0, HelpMessage = 'An array of menu definition objects. Each object should contain properties for Title, Items, StartRow, BorderInset, and Border.')]
            [Object[]]$MenuItems
        )

        function Get-MenuValue
            {
                param(
                    [object]$Menu,
                    [string]$Name,
                    $Default = $null
                )

                if ($null -eq $Menu)
                    {
                        return $Default
                    }

                if ($Menu -is [System.Collections.IDictionary])
                    {
                        if ($Menu.Contains($Name) -and $null -ne $Menu[$Name])
                            {
                                return $Menu[$Name]
                            }

                        return $Default
                    }

                $prop = $Menu.PSObject.Properties[$Name]
                if ($null -ne $prop -and $null -ne $prop.Value)
                    {
                        return $prop.Value
                    }

                return $Default
            }

        $NormalizedMenus = @()
        ForEach($Raw in $MenuItems)
            {
                If(-not ($Raw -is [System.Collections.IDictionary] -or $Raw -is [PSObject]))
                    {
                        throw "Each menu definition must be a hashtable or object. Invalid item: $($Raw | Out-String)"
                    }

                $MenuHash = @{}
                If($Raw -is [System.Collections.IDictionary])
                    {
                        $MenuHash = $Raw
                    }
                Else
                    {
                        ForEach($Property in $Raw.PSObject.Properties)
                            {
                                $MenuHash[$Property.Name] = $Property.Value
                            }
                    }

                $MenuIndex = $MenuItems.IndexOf($Raw)
                $NormalizedMenus += [PSCustomObject]@{
                    OriginalOrder = $MenuIndex
                    SortIndex = If($MenuHash.ContainsKey('Index') -and $null -ne $MenuHash.Index) { [int]$MenuHash.Index } else { $MenuIndex }
                    Menu = $MenuHash
                }
            }

        $Ordered = $NormalizedMenus | Sort-Object -Property SortIndex, OriginalOrder
        $IndexToPosition = @{}
        $TitleToPosition = @{}

        For($i = 0; $i -lt $Ordered.Count; $i++)
            {
                $sortIndex = [int]$Ordered[$i].SortIndex
                If(-not $IndexToPosition.ContainsKey($sortIndex))
                    {
                        $IndexToPosition[$sortIndex] = $i
                    }

                $menuTitle = [string](Get-MenuValue -Menu $Ordered[$i].Menu -Name 'Title' -Default "")
                If(-not [string]::IsNullOrEmpty($menuTitle))
                    {
                        $titleKey = $menuTitle.ToLowerInvariant()
                        If(-not $TitleToPosition.ContainsKey($titleKey))
                            {
                                $TitleToPosition[$titleKey] = $i
                            }
                    }
            }

        Function Resolve-TargetPosition
            {
                param(
                    [object]$Target
                )

                If($null -eq $Target)
                    {
                        Return $null
                    }

                If($Target -is [int] -or $Target -is [long] -or $Target -is [short] -or ($Target -is [string] -and $Target -match '^-?\d+$'))
                    {
                        $indexValue = [int]$Target
                        If($IndexToPosition.ContainsKey($indexValue))
                            {
                                Return [int]$IndexToPosition[$indexValue]
                            }

                        If($indexValue -ge 0 -and $indexValue -lt $Ordered.Count)
                            {
                                Return $indexValue
                            }

                        Return $null
                    }

                $titleValue = [string]$Target
                If([string]::IsNullOrWhiteSpace($titleValue))
                    {
                        Return $null
                    }

                $titleKey = $titleValue.ToLowerInvariant()
                If($TitleToPosition.ContainsKey($titleKey))
                    {
                        Return [int]$TitleToPosition[$titleKey]
                    }

                Return $null
            }

        Function Resolve-NextMenuPosition
            {
                param(
                    [object]$MenuDef,
                    [string]$SelectedItem,
                    [int]$CurrentPosition,
                    [Nullable[int]]$DefaultNextPosition
                )

                $nextBySelection = Get-MenuValue -Menu $MenuDef -Name 'NextBySelection' -Default $null
                If($null -ne $nextBySelection -and ($nextBySelection -is [System.Collections.IDictionary]))
                    {
                        If($nextBySelection.Contains($SelectedItem))
                            {
                                $resolved = Resolve-TargetPosition -Target $nextBySelection[$SelectedItem]
                                If($null -ne $resolved)
                                    {
                                        Return [Nullable[int]]$resolved
                                    }
                            }
                    }

                $nextIndex = Get-MenuValue -Menu $MenuDef -Name 'NextIndex' -Default $null
                If($null -ne $nextIndex)
                    {
                        $resolved = Resolve-TargetPosition -Target $nextIndex
                        If($null -ne $resolved)
                            {
                                Return [Nullable[int]]$resolved
                            }
                    }

                $nextTitle = Get-MenuValue -Menu $MenuDef -Name 'NextTitle' -Default $null
                If($null -ne $nextTitle)
                    {
                        $resolved = Resolve-TargetPosition -Target $nextTitle
                        If($null -ne $resolved)
                            {
                                Return [Nullable[int]]$resolved
                            }
                    }

                Return $DefaultNextPosition
            }

        $Selections = @{}
        $NavigationStack = New-Object 'System.Collections.Generic.List[int]'
        $CurrentMenuPos = 0

        While($CurrentMenuPos -ge 0 -and $CurrentMenuPos -lt $Ordered.Count)
            {
                $Underlay = $null

                For($j = 0; $j -lt $NavigationStack.Count; $j++)
                    {
                        $PathPosition = $NavigationStack[$j]
                        $MenuDef = $Ordered[$PathPosition].Menu
                        $OriginalOrder = $Ordered[$PathPosition].OriginalOrder

                        $Items       = @((Get-MenuValue -Menu $MenuDef -Name 'Items' -Default @()))
                        $Columns     = [int](Get-MenuValue -Menu $MenuDef -Name 'Columns' -Default 3)
                        $Padding     = [int](Get-MenuValue -Menu $MenuDef -Name 'Padding' -Default 1)
                        $FullWidth   = [bool](Get-MenuValue -Menu $MenuDef -Name 'FullWidth' -Default $true)
                        $Border      = [bool](Get-MenuValue -Menu $MenuDef -Name 'Border' -Default $true)
                        $Title       = [string](Get-MenuValue -Menu $MenuDef -Name 'Title' -Default $null)
                        $StartRow    = [int](Get-MenuValue -Menu $MenuDef -Name 'StartRow' -Default 0)
                        $BorderInset = [int](Get-MenuValue -Menu $MenuDef -Name 'BorderInset' -Default 0)
                        $BorderType  = [string](Get-MenuValue -Menu $MenuDef -Name 'BorderType' -Default 'Curved')

                        $SelectionColor = Resolve-ColorCode -ColorCode (Get-MenuValue -Menu $MenuDef -Name 'SelectionColor' -Default 'Magenta') -ColorLocation 'Foreground'
                        $TextColor      = Resolve-ColorCode -ColorCode (Get-MenuValue -Menu $MenuDef -Name 'TextColor' -Default 'White') -ColorLocation 'Foreground'
                        $BorderColor    = Resolve-ColorCode -ColorCode (Get-MenuValue -Menu $MenuDef -Name 'BorderColor' -Default 'Blue') -ColorLocation 'Foreground'
                        $TitleColor     = Resolve-ColorCode -ColorCode (Get-MenuValue -Menu $MenuDef -Name 'TitleColor' -Default 'Blue') -ColorLocation 'Foreground'

                        $BackgroundMenu = [ConsoleFX.GridMenu]::New(
                            $Items,
                            $Columns,
                            $Padding,
                            $FullWidth,
                            $Border,
                            ($BorderType | ConvertTo-Case -From Pascal -To Snake),
                            $Title,
                            $StartRow,
                            $BorderInset,
                            $true
                        )
                        $BackgroundMenu.SetColor($SelectionColor, $TextColor, $BorderColor, $TitleColor)

                        If($Selections.ContainsKey($OriginalOrder))
                            {
                                $BackgroundMenu.SetSelectedItem([string]$Selections[$OriginalOrder])
                            }

                        $BackgroundMenu.UnderlayMenu = $Underlay
                        $Underlay = $BackgroundMenu
                    }

                $ActiveDef = $Ordered[$CurrentMenuPos].Menu
                $ActiveOriginalOrder = $Ordered[$CurrentMenuPos].OriginalOrder

                $ActiveItems       = @((Get-MenuValue -Menu $ActiveDef -Name 'Items' -Default @()))
                $ActiveColumns     = [int](Get-MenuValue -Menu $ActiveDef -Name 'Columns' -Default 3)
                $ActivePadding     = [int](Get-MenuValue -Menu $ActiveDef -Name 'Padding' -Default 1)
                $ActiveFullWidth   = [bool](Get-MenuValue -Menu $ActiveDef -Name 'FullWidth' -Default $true)
                $ActiveBorder      = [bool](Get-MenuValue -Menu $ActiveDef -Name 'Border' -Default $true)
                $ActiveTitle       = [string](Get-MenuValue -Menu $ActiveDef -Name 'Title' -Default $null)
                $ActiveStartRow    = [int](Get-MenuValue -Menu $ActiveDef -Name 'StartRow' -Default 0)
                $ActiveBorderInset = [int](Get-MenuValue -Menu $ActiveDef -Name 'BorderInset' -Default 0)
                $ActiveBorderType  = [string](Get-MenuValue -Menu $ActiveDef -Name 'BorderType' -Default 'Curved')

                $ActiveSelectionColor = Resolve-ColorCode -ColorCode (Get-MenuValue -Menu $ActiveDef -Name 'SelectionColor' -Default 'Magenta') -ColorLocation 'Foreground'
                $ActiveTextColor      = Resolve-ColorCode -ColorCode (Get-MenuValue -Menu $ActiveDef -Name 'TextColor' -Default 'White') -ColorLocation 'Foreground'
                $ActiveBorderColor    = Resolve-ColorCode -ColorCode (Get-MenuValue -Menu $ActiveDef -Name 'BorderColor' -Default 'Blue') -ColorLocation 'Foreground'
                $ActiveTitleColor     = Resolve-ColorCode -ColorCode (Get-MenuValue -Menu $ActiveDef -Name 'TitleColor' -Default 'Blue') -ColorLocation 'Foreground'

                $ActiveMenu = [ConsoleFX.GridMenu]::New(
                    $ActiveItems,
                    $ActiveColumns,
                    $ActivePadding,
                    $ActiveFullWidth,
                    $ActiveBorder,
                    ($ActiveBorderType | ConvertTo-Case -From Pascal -To Snake),
                    $ActiveTitle,
                    $ActiveStartRow,
                    $ActiveBorderInset,
                    $false
                )
                $ActiveMenu.SetColor($ActiveSelectionColor, $ActiveTextColor, $ActiveBorderColor, $ActiveTitleColor)
                $ActiveMenu.UnderlayMenu = $Underlay

                if ($Selections.ContainsKey($ActiveOriginalOrder))
                    {
                        $ActiveMenu.SetSelectedItem([string]$Selections[$ActiveOriginalOrder])
                    }

                $Result = $ActiveMenu.Run()

                if ($ActiveMenu.WasCancelled)
                    {
                        if ($NavigationStack.Count -gt 0)
                            {
                                $PreviousPosition = $NavigationStack[$NavigationStack.Count - 1]
                                $NavigationStack.RemoveAt($NavigationStack.Count - 1)
                                $CurrentMenuPos = $PreviousPosition
                                continue
                            }

                        continue
                    }

                $Selections[$ActiveOriginalOrder] = $Result
                $DefaultNextPosition = if (($CurrentMenuPos + 1) -lt $Ordered.Count) { [Nullable[int]]($CurrentMenuPos + 1) } else { [Nullable[int]]$null }
                $NextPosition = Resolve-NextMenuPosition -MenuDef $ActiveDef -SelectedItem $Result -CurrentPosition $CurrentMenuPos -DefaultNextPosition $DefaultNextPosition

                if ($null -eq $NextPosition)
                    {
                        break
                    }

                if ([int]$NextPosition -ne $CurrentMenuPos)
                    {
                        $NavigationStack.Add($CurrentMenuPos)
                    }

                $CurrentMenuPos = [int]$NextPosition
            }

        $OrderedSelections = @()
        For($k = 0; $k -lt $Ordered.Count; $k++)
            {
                $OriginalOrder = $Ordered[$k].OriginalOrder
                $OrderedSelections += $Selections[$OriginalOrder]
            }

        Return @{
            MenuOrder = @($Ordered | ForEach-Object { $_.Menu.Title })
            SelectionsByOriginalOrder = $Selections
            OrderedSelections = $OrderedSelections
        }
    }
