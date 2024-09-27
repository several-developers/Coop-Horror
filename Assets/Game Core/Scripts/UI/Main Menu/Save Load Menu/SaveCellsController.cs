using GameCore.Gameplay.Other;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.UI.MainMenu.SaveSelectionMenu
{
    public class SaveCellsController : MonoBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------
        
        [Title(Constants.References)]
        [SerializeField, Required]
        private SaveCellView[] _saveCellsView;

        // FIELDS: --------------------------------------------------------------------------------
        
        private int _lastSelectedCellIndex;

        // GAME ENGINE METHODS: -------------------------------------------------------------------

        private void Awake()
        {
            _lastSelectedCellIndex = GameStaticState.SelectedSaveIndex;
            
            SetupCellsView();
            SelectNewCellView();
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------
        
        private void SetupCellsView()
        {
            int iterations = _saveCellsView.Length;

            for (int i = 0; i < iterations; i++)
            {
                SaveCellView saveCellView = _saveCellsView[i];
                saveCellView.SetCellIndex(i);
                
                saveCellView.OnSaveCellClickedEvent += OnSaveCellClicked;
            }
        }

        private void DeselectLastCellView() =>
            _saveCellsView[_lastSelectedCellIndex].Deselect();

        private void SelectNewCellView() =>
            _saveCellsView[_lastSelectedCellIndex].Select();

        // EVENTS RECEIVERS: ----------------------------------------------------------------------

        private void OnSaveCellClicked(int cellIndex)
        {
            DeselectLastCellView();
            
            _lastSelectedCellIndex = cellIndex;
            
            SelectNewCellView();
            
            Debug.Log($"Save Cell ({cellIndex}) clicked.");

            GameStaticState.SetSelectedSaveIndex(cellIndex);
        }
    }
}