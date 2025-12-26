using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ModernDesign.Localization;

namespace ModernDesign.MVVM.View
{
    public partial class CheatsGuideView : Window
    {
        private List<CheatItem> _allCheats = new List<CheatItem>();
        private string _selectedCategory = LanguageManager.Get("CheatsGuideView.AllCategory");
        private HashSet<string> _favoriteCommands = new HashSet<string>();

        public CheatsGuideView()
        {
            InitializeComponent();
            LoadFavorites();
            ApplyLanguage();
            InitializeCheats();
        }

        private void ApplyLanguage()
        {
            Title = LanguageManager.Get("CheatsGuide.Title");
            TitleText.Text = LanguageManager.Get("CheatsGuide.TitleText");
            SubtitleText.Text = LanguageManager.Get("CheatsGuide.SubtitleText");
            SearchBox.Text = LanguageManager.Get("CheatsGuide.SearchBox");
            ExportAllButton.Content = LanguageManager.Get("CheatsGuide.ExportAllButton");
            ExportAllButton.ToolTip = LanguageManager.Get("CheatsGuide.ExportAllButtonTooltip");
            ExportFavoritesButton.Content = LanguageManager.Get("CheatsGuide.ExportFavoritesButton");
            ExportFavoritesButton.ToolTip = LanguageManager.Get("CheatsGuide.ExportFavoritesButtonTooltip");
        }

        private void LoadFavorites()
        {
            try
            {
                string favoritesPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ModernDesign",
                    "CheatsFavorites.txt"
                );

                if (File.Exists(favoritesPath))
                {
                    var lines = File.ReadAllLines(favoritesPath);
                    _favoriteCommands = new HashSet<string>(lines);
                }
            }
            catch
            {
                // Si hay error, simplemente no cargamos favoritos
            }
        }

        private void SaveFavorites()
        {
            try
            {
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ModernDesign"
                );

                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                string favoritesPath = Path.Combine(appDataPath, "CheatsFavorites.txt");
                File.WriteAllLines(favoritesPath, _favoriteCommands);
            }
            catch
            {
                // Si hay error al guardar, ignoramos
            }
        }

        private void InitializeCheats()
        {
            bool es = LanguageManager.IsSpanish;

            _allCheats = new List<CheatItem>
            {
                // BASIC CHEATS
                new CheatItem
                {
                    Category = LanguageManager.Get("CheatsGuideView.BasicCategory"),
                    Command = "testingcheats true",
                    Name = LanguageManager.Get("CheatsGuideView.EnableCheatsName"),
                    Description = LanguageManager.Get("CheatsGuideView.EnableCheatsDescription"),
                    Usage = LanguageManager.Get("CheatsGuideView.EnableCheatsUsage")
                },
                new CheatItem
                {
                    Category = LanguageManager.Get("CheatsGuideView.BasicCategory"),
                    Command = "testingcheats false",
                    Name = LanguageManager.Get("CheatsGuideView.DisableCheatsName"),
                    Description = LanguageManager.Get("CheatsGuideView.DisableCheatsDescription"),
                    Usage = LanguageManager.Get("CheatsGuideView.DisableCheatsUsage")
                },
                new CheatItem
                {
                    Category = LanguageManager.Get("CheatsGuideView.BasicCategory"),
                    Command = "headlineeffects on/off",
                    Name = LanguageManager.Get("CheatsGuideView.HeadlineEffectsName"),
                    Description = LanguageManager.Get("CheatsGuideView.HeadlineEffectsDescription"),
                    Usage = LanguageManager.Get("CheatsGuideView.HeadlineEffectsUsage")
                },
                new CheatItem
                {
                    Category = LanguageManager.Get("CheatsGuideView.BasicCategory"),
                    Command = "fps on/off",
                    Name = LanguageManager.Get("CheatsGuideView.FPSName"),
                    Description = LanguageManager.Get("CheatsGuideView.FPSDescription"),
                    Usage = LanguageManager.Get("CheatsGuideView.FPSUsage")
                },
                new CheatItem
                {
                    Category = LanguageManager.Get("CheatsGuideView.BasicCategory"),
                    Command = "fullscreen",
                    Name = LanguageManager.Get("CheatsGuideView.FullscreenName"),
                    Description = LanguageManager.Get("CheatsGuideView.FullscreenDescription"),
                    Usage = LanguageManager.Get("CheatsGuideView.FullscreenUsage")
                },
                
                new CheatItem
                {
                Category = LanguageManager.Get("CheatsGuideView.BasicCategory"),
                Command = "hovereffects on/off",
                Name = LanguageManager.Get("CheatsGuideView.HoverEffectsName"),
                Description = LanguageManager.Get("CheatsGuideView.HoverEffectsDescription"),
                Usage = "hovereffects on / hovereffects off"
            },

// MONEY CHEATS
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.MoneyCategory"),
                Command = "motherlode",
                Name = LanguageManager.Get("CheatsGuideView.MotherlodeName"),
                Description = LanguageManager.Get("CheatsGuideView.MotherlodeDescription"),
                Usage = "motherlode"
            },
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.MoneyCategory"),
                Command = "kaching",
                Name = LanguageManager.Get("CheatsGuideView.KachingName"),
                Description = LanguageManager.Get("CheatsGuideView.KachingDescription"),
                Usage = "kaching"
            },
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.MoneyCategory"),
                Command = "rosebud",
                Name = LanguageManager.Get("CheatsGuideView.RosebudName"),
                Description = LanguageManager.Get("CheatsGuideView.RosebudDescription"),
                Usage = "rosebud"
            },
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.MoneyCategory"),
                Command = "money [cantidad]",
                Name = LanguageManager.Get("CheatsGuideView.ExactMoneyName"),
                Description = LanguageManager.Get("CheatsGuideView.ExactMoneyDescription"),
                Usage = "money 1000000 (replace with desired amount)"
            },
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.MoneyCategory"),
                Command = "household.autopay_bills true/false",
                Name = LanguageManager.Get("CheatsGuideView.AutoPayBillsName"),
                Description = LanguageManager.Get("CheatsGuideView.AutoPayBillsDescription"),
                Usage = "household.autopay_bills true / false"
            },
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.MoneyCategory"),
                Command = "FreeRealEstate on/off",
                Name = LanguageManager.Get("CheatsGuideView.FreeRealEstateName"),
                Description = LanguageManager.Get("CheatsGuideView.FreeRealEstateDescription"),
                Usage = "FreeRealEstate on / off"
            },

// NEEDS CHEATS
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.NeedsCategory"),
                Command = "fillmotive motive_[tipo]",
                Name = LanguageManager.Get("CheatsGuideView.FillNeedName"),
                Description = LanguageManager.Get("CheatsGuideView.FillNeedDescription"),
                Usage = "fillmotive motive_hunger"
            },
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.NeedsCategory"),
                Command = "sims.fill_all_commodities",
                Name = LanguageManager.Get("CheatsGuideView.FillAllNeedsName"),
                Description = LanguageManager.Get("CheatsGuideView.FillAllNeedsDescription"),
                Usage = "sims.fill_all_commodities"
            },
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.NeedsCategory"),
                Command = "sims.disable_all_commodities",
                Name = LanguageManager.Get("CheatsGuideView.DisableNeedsDecayName"),
                Description = LanguageManager.Get("CheatsGuideView.DisableNeedsDecayDescription"),
                Usage = "sims.disable_all_commodities"
            },
            new CheatItem
            {
                Category = LanguageManager.Get("CheatsGuideView.NeedsCategory"),
                Command = "sims.enable_all_commodities",
                Name = LanguageManager.Get("CheatsGuideView.EnableNeedsDecayName"),
                Description = LanguageManager.Get("CheatsGuideView.EnableNeedsDecayDescription"),
                Usage = "sims.enable_all_commodities"
            }
,

                // SKILLS CHEATS
                new CheatItem
                {
                    Category = LanguageManager.Get("CheatsGuideView.SkillsCategory"),
                    Command = "stats.set_skill_level Major_[habilidad] [nivel]",
                    Name = LanguageManager.Get("CheatsGuideView.SetMajorSkillName"),
                    Description = LanguageManager.Get("CheatsGuideView.SetMajorSkillDescription"),
                    Usage = "stats.set_skill_level Major_Painting 10"
                },
                new CheatItem
                {
                    Category = LanguageManager.Get("CheatsGuideView.SkillsCategory"),
                    Command = "stats.set_skill_level Minor_[habilidad] [nivel]",
                    Name = LanguageManager.Get("CheatsGuideView.SetMinorSkillName"),
                    Description = LanguageManager.Get("CheatsGuideView.SetMinorSkillDescription"),
                    Usage = "stats.set_skill_level Minor_Dancing 5"
                },
                new CheatItem
                {
                    Category = LanguageManager.Get("CheatsGuideView.SkillsCategory"),
                    Command = "stats.set_skill_level Skill_Child_[habilidad] [nivel]",
                    Name = LanguageManager.Get("CheatsGuideView.ChildSkillName"),
                    Description = LanguageManager.Get("CheatsGuideView.ChildSkillDescription"),
                    Usage = "stats.set_skill_level Skill_Child_Creativity 10"
                },
                new CheatItem
                {
                    Category = LanguageManager.Get("CheatsGuideView.SkillsCategory"),
                    Command = "stats.set_skill_level Skill_Toddler_[habilidad] [nivel]",
                    Name = LanguageManager.Get("CheatsGuideView.ToddlerSkillName"),
                    Description = LanguageManager.Get("CheatsGuideView.ToddlerSkillDescription"),
                    Usage = "stats.set_skill_level Skill_Toddler_Thinking 5"
                }
,
// CAREER CHEATS
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.CareersCategory"),
    Command = "careers.promote [carrera]",
    Name = LanguageManager.Get("CheatsGuideView.InstantPromotionName"),
    Description = LanguageManager.Get("CheatsGuideView.InstantPromotionDescription"),
    Usage = "careers.promote Adult_Painter"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.CareersCategory"),
    Command = "careers.demote [carrera]",
    Name = LanguageManager.Get("CheatsGuideView.DemoteCareerName"),
    Description = LanguageManager.Get("CheatsGuideView.DemoteCareerDescription"),
    Usage = "careers.demote Adult_Painter"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.CareersCategory"),
    Command = "careers.add_career [carrera]",
    Name = LanguageManager.Get("CheatsGuideView.AddCareerName"),
    Description = LanguageManager.Get("CheatsGuideView.AddCareerDescription"),
    Usage = "careers.add_career Adult_Painter"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.CareersCategory"),
    Command = "careers.remove_career [carrera]",
    Name = LanguageManager.Get("CheatsGuideView.RemoveCareerName"),
    Description = LanguageManager.Get("CheatsGuideView.RemoveCareerDescription"),
    Usage = "careers.remove_career Adult_Painter"
}
,

// RELATIONSHIPS CHEATS
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.RelationshipsCategory"),
    Command = "modifyrelationship [Sim1] [Sim2] [cantidad] LTR_Friendship_Main",
    Name = LanguageManager.Get("CheatsGuideView.ModifyFriendshipName"),
    Description = LanguageManager.Get("CheatsGuideView.ModifyFriendshipDescription"),
    Usage = "modifyrelationship John Doe Jane Doe 100 LTR_Friendship_Main"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.RelationshipsCategory"),
    Command = "modifyrelationship [Sim1] [Sim2] [cantidad] LTR_Romance_Main",
    Name = LanguageManager.Get("CheatsGuideView.ModifyRomanceName"),
    Description = LanguageManager.Get("CheatsGuideView.ModifyRomanceDescription"),
    Usage = "modifyrelationship John Doe Jane Doe 100 LTR_Romance_Main"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.RelationshipsCategory"),
    Command = "relationship.introduce_sim_to_all_others",
    Name = LanguageManager.Get("CheatsGuideView.MeetEveryoneName"),
    Description = LanguageManager.Get("CheatsGuideView.MeetEveryoneDescription"),
    Usage = "relationship.introduce_sim_to_all_others"
},

// BUILD/BUY CHEATS
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.BuildCategory"),
    Command = "bb.moveobjects on/off",
    Name = LanguageManager.Get("CheatsGuideView.MoveObjectsName"),
    Description = LanguageManager.Get("CheatsGuideView.MoveObjectsDescription"),
    Usage = "bb.moveobjects on"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.BuildCategory"),
    Command = "bb.showhiddenobjects",
    Name = LanguageManager.Get("CheatsGuideView.HiddenObjectsName"),
    Description = LanguageManager.Get("CheatsGuideView.HiddenObjectsDescription"),
    Usage = "bb.showhiddenobjects"
}
,
               new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.BuildCategory"),
    Command = "bb.showliveeditobjects",
    Name = LanguageManager.Get("CheatsGuideView.LiveEditObjectsName"),
    Description = LanguageManager.Get("CheatsGuideView.LiveEditObjectsDescription"),
    Usage = "bb.showliveeditobjects"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.BuildCategory"),
    Command = "bb.ignoregameplayunlocksentitlement",
    Name = LanguageManager.Get("CheatsGuideView.UnlockCareerObjectsName"),
    Description = LanguageManager.Get("CheatsGuideView.UnlockCareerObjectsDescription"),
    Usage = "bb.ignoregameplayunlocksentitlement"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.BuildCategory"),
    Command = "bb.enablefreebuild",
    Name = LanguageManager.Get("CheatsGuideView.FreeBuildName"),
    Description = LanguageManager.Get("CheatsGuideView.FreeBuildDescription"),
    Usage = "bb.enablefreebuild"
},

// CAS CHEATS
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.CASCategory"),
    Command = "cas.fulleditmode",
    Name = LanguageManager.Get("CheatsGuideView.FullEditModeCASName"),
    Description = LanguageManager.Get("CheatsGuideView.FullEditModeCASDescription"),
    Usage = "cas.fulleditmode"
},

// DEATH & LIFE CHEATS
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.LifeDeathCategory"),
    Command = "death.toggle true/false",
    Name = LanguageManager.Get("CheatsGuideView.DisableDeathName"),
    Description = LanguageManager.Get("CheatsGuideView.DisableDeathDescription"),
    Usage = "death.toggle false (desactiva muerte / disables death)"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.LifeDeathCategory"),
    Command = "traits.equip_trait Ghost_[tipo]",
    Name = LanguageManager.Get("CheatsGuideView.MakeGhostName"),
    Description = LanguageManager.Get("CheatsGuideView.MakeGhostDescription"),
    Usage = "traits.equip_trait Ghost_OldAge"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.LifeDeathCategory"),
    Command = "traits.remove_trait Ghost_[tipo]",
    Name = LanguageManager.Get("CheatsGuideView.RemoveGhostName"),
    Description = LanguageManager.Get("CheatsGuideView.RemoveGhostDescription"),
    Usage = "traits.remove_trait Ghost_OldAge"
},

// ASPIRATION & SATISFACTION CHEATS
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.AspirationsCategory"),
    Command = "aspirations.complete_current_milestone",
    Name = LanguageManager.Get("CheatsGuideView.CompleteCurrentMilestoneName"),
    Description = LanguageManager.Get("CheatsGuideView.CompleteCurrentMilestoneDescription"),
    Usage = "aspirations.complete_current_milestone"
}
,
            new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.AspirationsCategory"),
    Command = "sims.give_satisfaction_points [cantidad]",
    Name = LanguageManager.Get("CheatsGuideView.SatisfactionPointsName"),
    Description = LanguageManager.Get("CheatsGuideView.SatisfactionPointsDescription"),
    Usage = "sims.give_satisfaction_points 5000"
},

// TRAITS CHEATS
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.TraitsCategory"),
    Command = "traits.equip_trait [rasgo]",
    Name = LanguageManager.Get("CheatsGuideView.AddTraitName"),
    Description = LanguageManager.Get("CheatsGuideView.AddTraitDescription"),
    Usage = "traits.equip_trait Creative"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.TraitsCategory"),
    Command = "traits.remove_trait [rasgo]",
    Name = LanguageManager.Get("CheatsGuideView.RemoveTraitName"),
    Description = LanguageManager.Get("CheatsGuideView.RemoveTraitDescription"),
    Usage = "traits.remove_trait Creative"
},

// MISC CHEATS
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.MiscCategory"),
    Command = "sims.hard_reset",
    Name = LanguageManager.Get("CheatsGuideView.ResetSimName"),
    Description = LanguageManager.Get("CheatsGuideView.ResetSimDescription"),
    Usage = "sims.hard_reset"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.MiscCategory"),
    Command = "sims.spawnsimple [ID]",
    Name = LanguageManager.Get("CheatsGuideView.SpawnObjectName"),
    Description = LanguageManager.Get("CheatsGuideView.SpawnObjectDescription"),
    Usage = "sims.spawnsimple"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.MiscCategory"),
    Command = "clock.advance_game_time [horas] [minutos] [segundos]",
    Name = LanguageManager.Get("CheatsGuideView.AdvanceTimeName"),
    Description = LanguageManager.Get("CheatsGuideView.AdvanceTimeDescription"),
    Usage = "clock.advance_game_time 8 0 0"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.MiscCategory"),
    Command = "sims.add_buff [buff]",
    Name = LanguageManager.Get("CheatsGuideView.AddMoodletName"),
    Description = LanguageManager.Get("CheatsGuideView.AddMoodletDescription"),
    Usage = "sims.add_buff e_buff_happy"
},
new CheatItem
{
    Category = LanguageManager.Get("CheatsGuideView.MiscCategory"),
    Command = "sims.remove_all_buffs",
    Name = LanguageManager.Get("CheatsGuideView.RemoveAllMoodletsName"),
    Description = LanguageManager.Get("CheatsGuideView.RemoveAllMoodletsDescription"),
    Usage = "sims.remove_all_buffs"
}

            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateCategoryButtons();
            DisplayCheats();
        }

        private void CreateCategoryButtons()
        {
            bool es = LanguageManager.IsSpanish;

            // Get unique categories
            var categories = _allCheats.Select(c => c.Category).Distinct().OrderBy(c => c).ToList();
            categories.Insert(0, LanguageManager.Get("CheatsGuideView.AllCategory"));

            // Add Favorites category
            categories.Insert(1, LanguageManager.Get("CheatsGuideView.FavoritesCategory"));

            foreach (var category in categories)
            {
                Button btn = new Button
                {
                    Content = category,
                    Style = (Style)FindResource("CategoryButton"),
                    Tag = category
                };

                if (category == LanguageManager.Get("CheatsGuideView.AllCategory"))
                {
                    btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1"));
                }

                btn.Click += CategoryButton_Click;
                CategoryPanel.Children.Add(btn);
            }
        }

        private void CategoryButton_Click(object sender, RoutedEventArgs e)
        {
             Button btn = sender as Button;
            _selectedCategory = btn.Tag.ToString();

            // Update button colors
            foreach (Button categoryBtn in CategoryPanel.Children)
            {
                if (categoryBtn.Tag.ToString() == _selectedCategory)
                {
                    categoryBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1"));
                }
                else
                {
                    categoryBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E293B"));
                }
            }

            DisplayCheats();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            DisplayCheats();
        }

        private void DisplayCheats()
        {
            bool es = LanguageManager.IsSpanish;
            CheatsPanel.Children.Clear();

            string searchText = SearchBox.Text.ToLower();
            var filteredCheats = _allCheats.AsEnumerable();

            // Filter by category
            if (_selectedCategory == LanguageManager.Get("CheatsGuideView.FavoritesCategory"))
            {
                filteredCheats = filteredCheats.Where(c => _favoriteCommands.Contains(c.Command));
            }
            else if (_selectedCategory != LanguageManager.Get("CheatsGuideView.AllCategory"))
            {
                filteredCheats = filteredCheats.Where(c => c.Category == _selectedCategory);
            }

            // Filter by search
            if (!string.IsNullOrWhiteSpace(searchText) && searchText != LanguageManager.Get("CheatsGuide.SearchBox").ToLower())
            {
                filteredCheats = filteredCheats.Where(c =>
                    c.Command.ToLower().Contains(searchText) ||
                    c.Name.ToLower().Contains(searchText) ||
                    c.Description.ToLower().Contains(searchText));
            }

            foreach (var cheat in filteredCheats)
            {
                CreateCheatCard(cheat);
            }

            if (!filteredCheats.Any())
            {
                TextBlock noResults = new TextBlock
                {
                    Text = LanguageManager.Get("CheatsGuideView.NoCheatsFound"),
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                    FontSize = 16,
                    FontFamily = new FontFamily("Bahnschrift Light"),
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 50, 0, 0)
                };
                CheatsPanel.Children.Add(noResults);
            }
        }

        private void CreateCheatCard(CheatItem cheat)
        {
            bool es = LanguageManager.IsSpanish;

            Border card = new Border
            {
                Style = (Style)FindResource("CheatCard")
            };

            Grid cardGrid = new Grid();
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            cardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Info Panel
            StackPanel infoPanel = new StackPanel();

            // Category Badge
            Border categoryBadge = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6366F1")),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 8)
            };
            TextBlock categoryText = new TextBlock
            {
                Text = cheat.Category,
                Foreground = Brushes.White,
                FontSize = 10,
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontWeight = FontWeights.Bold
            };
            categoryBadge.Child = categoryText;
            infoPanel.Children.Add(categoryBadge);

            // Name
            TextBlock nameText = new TextBlock
            {
                Text = cheat.Name,
                Foreground = Brushes.White,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 0, 0, 8)
            };
            nameText.Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                ShadowDepth = 1,
                Opacity = 0.8,
                BlurRadius = 6
            };
            infoPanel.Children.Add(nameText);

            // Command
            Border commandBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 0, 10)
            };
            TextBlock commandText = new TextBlock
            {
                Text = cheat.Command,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#22C55E")),
                FontSize = 14,
                FontFamily = new FontFamily("Consolas"),
                FontWeight = FontWeights.Bold
            };
            commandBorder.Child = commandText;
            infoPanel.Children.Add(commandBorder);

            // Description
            TextBlock descText = new TextBlock
            {
                Text = cheat.Description,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB")),
                FontSize = 13,
                FontFamily = new FontFamily("Bahnschrift Light"),
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 8)
            };
            infoPanel.Children.Add(descText);

            // Usage
            TextBlock usageText = new TextBlock
            {
                Text = cheat.Usage,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8")),
                FontSize = 12,
                FontFamily = new FontFamily("Bahnschrift Light"),
                FontStyle = FontStyles.Italic,
                TextWrapping = TextWrapping.Wrap
            };
            infoPanel.Children.Add(usageText);

            Grid.SetColumn(infoPanel, 0);
            cardGrid.Children.Add(infoPanel);

            // Action Buttons Panel
            StackPanel actionPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(10, 0, 0, 0)
            };

            // Favorite Button
            bool isFavorite = _favoriteCommands.Contains(cheat.Command);
            Button favoriteBtn = new Button
            {
                Content = isFavorite ? "⭐" : "☆",
                Style = (Style)FindResource("FavoriteButton"),
                Tag = cheat.Command,
                ToolTip = es ? "Marcar como favorito" : "Mark as favorite",
                Margin = new Thickness(0, 0, 0, 5)
            };

            if (isFavorite)
            {
                favoriteBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCD34D"));
            }

            favoriteBtn.Click += FavoriteButton_Click;
            actionPanel.Children.Add(favoriteBtn);

            // Copy Button
            Button copyBtn = new Button
            {
                Content = "📋",
                Style = (Style)FindResource("CopyButton"),
                Tag = cheat.Command,
                ToolTip = es ? "Copiar al portapapeles" : "Copy to clipboard"
            };
            copyBtn.Click += CopyButton_Click;
            actionPanel.Children.Add(copyBtn);

            Grid.SetColumn(actionPanel, 1);
            cardGrid.Children.Add(actionPanel);

            card.Child = cardGrid;
            CheatsPanel.Children.Add(card);
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string command = btn.Tag.ToString();

            if (_favoriteCommands.Contains(command))
            {
                _favoriteCommands.Remove(command);
                btn.Content = "☆";
                btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
            }
            else
            {
                _favoriteCommands.Add(command);
                btn.Content = "⭐";
                btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCD34D"));
            }

            SaveFavorites();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            bool es = LanguageManager.IsSpanish;
            Button btn = sender as Button;
            string command = btn.Tag.ToString();

            try
            {
                System.Windows.Clipboard.SetText(command);
                btn.Content = "✅";

                // Reset after 2 seconds
                System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() => btn.Content = "📋");
                });
            }
            catch
            {
                MessageBox.Show(
                    es ? "Error al copiar al portapapeles" : "Error copying to clipboard",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ExportAllButton_Click(object sender, RoutedEventArgs e)
        {
            ExportCheats(_allCheats, "Sims4_AllCheats.txt");
        }

        private void ExportFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            var favoriteCheats = _allCheats.Where(c => _favoriteCommands.Contains(c.Command)).ToList();

            if (!favoriteCheats.Any())
            {
                bool es = LanguageManager.IsSpanish;
                MessageBox.Show(
                    es ? "No tienes trucos favoritos marcados." : "You don't have any favorite cheats marked.",
                    es ? "Sin Favoritos" : "No Favorites",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            ExportCheats(favoriteCheats, "Sims4_FavoriteCheats.txt");
        }

        private void ExportCheats(List<CheatItem> cheats, string fileName)
        {
            bool es = LanguageManager.IsSpanish;

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = Path.Combine(desktopPath, fileName);

                StringBuilder sb = new StringBuilder();

                // Header
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("          THE SIMS 4 - CHEATS GUIDE");
                sb.AppendLine("          Generated by Leuan's ToolKit");
                sb.AppendLine($"          {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine();
                sb.AppendLine($"Total Cheats: {cheats.Count}");
                sb.AppendLine();

                // Group by category
                var groupedCheats = cheats.GroupBy(c => c.Category).OrderBy(g => g.Key);

                foreach (var group in groupedCheats)
                {
                    sb.AppendLine();
                    sb.AppendLine("═══════════════════════════════════════════════════════════════");
                    sb.AppendLine($"  CATEGORY: {group.Key.ToUpper()}");
                    sb.AppendLine("═══════════════════════════════════════════════════════════════");
                    sb.AppendLine();

                    foreach (var cheat in group)
                    {
                        sb.AppendLine($"┌─ {cheat.Name}");
                        sb.AppendLine($"│");
                        sb.AppendLine($"│  Command:");
                        sb.AppendLine($"│  → {cheat.Command}");
                        sb.AppendLine($"│");
                        sb.AppendLine($"│  Description:");
                        sb.AppendLine($"│  {WrapText(cheat.Description, 60)}");
                        sb.AppendLine($"│");
                        sb.AppendLine($"│  Usage:");
                        sb.AppendLine($"│  {WrapText(cheat.Usage, 60)}");
                        sb.AppendLine($"└───────────────────────────────────────────────────────────");
                        sb.AppendLine();
                    }
                }

                // Footer
                sb.AppendLine();
                sb.AppendLine("═══════════════════════════════════════════════════════════════");
                sb.AppendLine("  HOW TO USE CHEATS:");
                sb.AppendLine("  1. Press Ctrl + Shift + C to open the cheat console");
                sb.AppendLine("  2. Type 'testingcheats true' and press Enter");
                sb.AppendLine("  3. Type your desired cheat and press Enter");
                sb.AppendLine("═══════════════════════════════════════════════════════════════");

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

                MessageBox.Show(
                    es ? $"Archivo exportado exitosamente:\n{filePath}" : $"File exported successfully:\n{filePath}",
                    es ? "Exportación Exitosa" : "Export Successful",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    es ? $"Error al exportar: {ex.Message}" : $"Error exporting: {ex.Message}",
                    es ? "Error" : "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private string WrapText(string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxWidth)
                return text;

            var words = text.Split(' ');
            var lines = new List<string>();
            var currentLine = new StringBuilder();

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 > maxWidth)
                {
                    if (currentLine.Length > 0)
                    {
                        lines.Add(currentLine.ToString());
                        currentLine.Clear();
                    }
                }

                if (currentLine.Length > 0)
                    currentLine.Append(" ");
                currentLine.Append(word);
            }

            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            return string.Join("\n│  ", lines);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }

    // Clase auxiliar para los cheats
    public class CheatItem
    {
        public string Category { get; set; }
        public string Command { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
    }
}