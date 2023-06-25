using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Disciples.Avalonia;

/// <summary>
/// ����, ������� ����������� ��������.
/// </summary>
/// <remarks>
/// ����������, ��� ��� ������������ �������� ������� ��� ����������� �����.
/// ���� ������� �� ����������� ������� ����. ������� ������ ������������� ���������� ����, ������� ��� ������� �������������.
/// </remarks>
public partial class OverlapWindow : Window
{
    /// <summary>
    /// ������� ������ ���� <see cref="OverlapWindow" />.
    /// </summary>
    public OverlapWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}