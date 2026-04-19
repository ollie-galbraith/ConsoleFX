namespace ConsoleFX
    {
        public static class GridConverter
            {
                public static GridManager FromList(List<string> items, int targetColumns, int defaultGap = 2)
                    {
                        if (items == null || items.Count == 0) return new GridManager(0,0, defaultGap);

                        int maxWidth = Console.WindowWidth;
                        int currentColumns = targetColumns;

                        while (currentColumns > 0)
                            {
                                int rows = (int)Math.Ceiling((double)items.Count / currentColumns);
                                var trialGrid = new GridManager(rows, currentColumns, defaultGap);

                                for (int i = 0; i < items.Count; i++)
                                    {
                                        trialGrid.SetCellValue(items[i], i / currentColumns, i % currentColumns);
                                    }

                                if (DoesGridFit(trialGrid, maxWidth))
                                    {
                                        return trialGrid;
                                    }
                                currentColumns--;
                            }

                        return new GridManager(0,0, defaultGap);
                    }

                public static GridManager FromTable(List<List<string>> tableData, int defaultGap = 2)
                    {
                        if (tableData == null || tableData.Count == 0) return new GridManager(0,0);

                        int rows = tableData.Count;
                        int columns = 0;
                        
                        foreach (var row in tableData)
                            {
                                if (row.Count > columns) columns = row.Count;
                            }
                        
                        var grid = new GridManager(rows, columns, defaultGap);

                        for (int row = 0; row < rows; row++)
                            {
                                for (int column = 0; column < tableData[row].Count; column++)
                                    {
                                        grid.SetCellValue(tableData[row][column], row, column);
                                    }
                            }

                        return grid;
                    }

                private static bool DoesGridFit(GridManager grid, int maxWidth)
                    {
                        // Trigger the width calculation
                        grid.UpdateWidths(); 

                        // Calculate the width of the widest possible row
                        int gridContentWidth = 0;

                        for (int c = 0; c < grid.Columns; c++)
                            {
                                gridContentWidth += grid._cachedWidths[c];
                                
                                // Add the LARGEST gap found in this column across all rows
                                if (c < grid.Columns - 1)
                                    {
                                        int maxGapForCol = 0;
                                        for (int r = 0; r < grid.Rows; r++)
                                            {
                                                // You'll need to make _spacing internal or add a getter
                                                if (grid.GetSpacing(r, c) > maxGapForCol)
                                                    maxGapForCol = grid.GetSpacing(r, c);
                                            }
                                        gridContentWidth += maxGapForCol;
                                    }
                            }

                        int totalWidth = gridContentWidth + (grid.Padding * 2);
                        return totalWidth <= maxWidth;
                    }
            }
    }