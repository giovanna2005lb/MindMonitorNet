using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace MindMonitor
{
    public class MainForm : Form
    {
        TabControl tabs;
        TextBox txtColaborador;
        ComboBox cbDesmotivacao, cbSobrecarga, cbEstresse;
        Button btnSalvarRegistro;

        DataGridView dgvRelatorio;
        Button btnCarregarRelatorio;
        Label lblMedias;

        public MainForm()
        {
            Text = "MindMonitor";
            Width = 900;
            Height = 600;

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            tabs = new TabControl() { Dock = DockStyle.Fill };

            var tabRegistro = new TabPage("Registro Diário");
            var tabRelatorio = new TabPage("Relatório Semanal (Gestor)");

            var pnlRegistro = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var lblNome = new Label() { Text = "Colaborador:", Location = new Point(10, 20) };
            txtColaborador = new TextBox() { Location = new Point(120, 17), Width = 250 };

            var lblDes = new Label() { Text = "Desmotivação (1-5):", Location = new Point(10, 60) };
            cbDesmotivacao = new ComboBox() { Location = new Point(150, 57), Width = 50, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblSob = new Label() { Text = "Sobrecarga (1-5):", Location = new Point(10, 100) };
            cbSobrecarga = new ComboBox() { Location = new Point(150, 97), Width = 50, DropDownStyle = ComboBoxStyle.DropDownList };

            var lblEst = new Label() { Text = "Estresse (1-5):", Location = new Point(10, 140) };
            cbEstresse = new ComboBox() { Location = new Point(150, 137), Width = 50, DropDownStyle = ComboBoxStyle.DropDownList };

            for (int i = 1; i <= 5; i++)
            {
                cbDesmotivacao.Items.Add(i);
                cbSobrecarga.Items.Add(i);
                cbEstresse.Items.Add(i);
            }
            cbDesmotivacao.SelectedIndex = 2;
            cbSobrecarga.SelectedIndex = 2;
            cbEstresse.SelectedIndex = 2;

            btnSalvarRegistro = new Button() { Text = "Salvar Registro", Location = new Point(10, 190), Width = 200 };
            btnSalvarRegistro.Click += BtnSalvarRegistro_Click;

            pnlRegistro.Controls.AddRange(new Control[]{ lblNome, txtColaborador, lblDes, cbDesmotivacao, lblSob, cbSobrecarga, lblEst, cbEstresse, btnSalvarRegistro });
            tabRegistro.Controls.Add(pnlRegistro);

            var pnlRel = new Panel() { Dock = DockStyle.Fill, Padding = new Padding(10) };
            btnCarregarRelatorio = new Button() { Text = "Carregar Relatório (últimos 7 dias)", Location = new Point(10, 10), Width = 250 };
            btnCarregarRelatorio.Click += BtnCarregarRelatorio_Click;

            dgvRelatorio = new DataGridView() { Location = new Point(10, 50), Width = 840, Height = 420, ReadOnly = true };
            lblMedias = new Label() { Location = new Point(270, 12), AutoSize = true };

            pnlRel.Controls.AddRange(new Control[]{ btnCarregarRelatorio, lblMedias, dgvRelatorio });
            tabRelatorio.Controls.Add(pnlRel);

            tabs.TabPages.Add(tabRegistro);
            tabs.TabPages.Add(tabRelatorio);

            Controls.Add(tabs);
        }

        private void BtnSalvarRegistro_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtColaborador.Text))
            {
                MessageBox.Show("Informe o nome do colaborador.");
                return;
            }

            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO RegistroDiario (Colaborador, Data, Desmotivacao, Sobrecarga, Estresse)
VALUES (@c, @d, @de, @so, @es)";
            cmd.Parameters.AddWithValue("@c", txtColaborador.Text.Trim());
            cmd.Parameters.AddWithValue("@d", DateTime.Now.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@de", cbDesmotivacao.SelectedItem);
            cmd.Parameters.AddWithValue("@so", cbSobrecarga.SelectedItem);
            cmd.Parameters.AddWithValue("@es", cbEstresse.SelectedItem);
            cmd.ExecuteNonQuery();

            MessageBox.Show("Registro salvo!");
        }

        private void BtnCarregarRelatorio_Click(object? sender, EventArgs e)
        {
            var fim = DateTime.Now.Date;
            var inicio = fim.AddDays(-6);

            using var conn = Database.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM RegistroDiario WHERE date(Data) BETWEEN date(@i) AND date(@f)";
            cmd.Parameters.AddWithValue("@i", inicio.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@f", fim.ToString("yyyy-MM-dd"));

            using var da = new SQLiteDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            dgvRelatorio.DataSource = dt;

            if (dt.Rows.Count == 0)
            {
                lblMedias.Text = "Nenhum registro.";
                return;
            }

            double mDes = 0, mSob = 0, mEst = 0;
            foreach (DataRow r in dt.Rows)
            {
                mDes += Convert.ToInt32(r["Desmotivacao"]);
                mSob += Convert.ToInt32(r["Sobrecarga"]);
                mEst += Convert.ToInt32(r["Estresse"]);
            }

            int n = dt.Rows.Count;
            lblMedias.Text = $"Médias — Desmotivação: {mDes/n:F1} | Sobrecarga: {mSob/n:F1} | Estresse: {mEst/n:F1}";
        }
    }
}
