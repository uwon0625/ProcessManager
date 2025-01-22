using System.Windows.Forms;
using System.Drawing;

namespace ProcessManager
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lstProcesses = new System.Windows.Forms.ListView();
            this.lstBlacklist = new System.Windows.Forms.ListView();
            this.btnAddToBlacklist = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSearch = new System.Windows.Forms.TextBox();

            // Form settings
            this.Text = "Process Manager";
            this.Size = new System.Drawing.Size(840, 660);

            // Labels
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Size = new System.Drawing.Size(120, 23);
            this.label1.Text = "Running Processes";
            this.label1.TextAlign = ContentAlignment.MiddleLeft;

            this.label2.Location = new System.Drawing.Point(428, 15);
            this.label2.Size = new System.Drawing.Size(150, 23);
            this.label2.Text = "Blacklist";
            this.label2.TextAlign = ContentAlignment.MiddleLeft;

            // Search box - positioned to the right of label1
            this.txtSearch.Location = new System.Drawing.Point(142, 15);
            this.txtSearch.Size = new System.Drawing.Size(220, 23);
            this.txtSearch.PlaceholderText = "Search processes...";
            this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);

            // Running Processes ListView - move back up
            this.lstProcesses.Location = new System.Drawing.Point(12, 45);
            this.lstProcesses.Size = new System.Drawing.Size(350, 520);
            this.lstProcesses.View = View.Details;
            this.lstProcesses.Scrollable = true;
            this.lstProcesses.MultiSelect = false;
            this.lstProcesses.GridLines = true;
            this.lstProcesses.HeaderStyle = ColumnHeaderStyle.None;
            this.lstProcesses.Columns.Add("Process", this.lstProcesses.Width - 4, HorizontalAlignment.Left);

            // Blacklist ListView
            this.lstBlacklist.Location = new System.Drawing.Point(428, 45);
            this.lstBlacklist.Size = new System.Drawing.Size(350, 520);
            this.lstBlacklist.View = View.Details;
            this.lstBlacklist.Scrollable = true;
            this.lstBlacklist.MultiSelect = false;
            this.lstBlacklist.GridLines = true;
            this.lstBlacklist.HeaderStyle = ColumnHeaderStyle.None;
            this.lstBlacklist.Columns.Add("Process", this.lstBlacklist.Width - 4, HorizontalAlignment.Left);

            // Add to Blacklist button
            this.btnAddToBlacklist.Location = new System.Drawing.Point(368, 180);
            this.btnAddToBlacklist.Size = new System.Drawing.Size(54, 54); // Made button bigger
            this.btnAddToBlacklist.Text = ">";
            this.btnAddToBlacklist.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.btnAddToBlacklist.Click += new System.EventHandler(this.btnAddToBlacklist_Click);

            // Add controls to form
            this.Controls.AddRange(new Control[] {
                this.lstProcesses,
                this.lstBlacklist,
                this.btnAddToBlacklist,
                this.label1,
                this.label2,
                this.txtSearch
            });
        }

        private ListView lstProcesses;
        private ListView lstBlacklist;
        private Button btnAddToBlacklist;
        private Label label1;
        private Label label2;
        private TextBox txtSearch;
    }
}