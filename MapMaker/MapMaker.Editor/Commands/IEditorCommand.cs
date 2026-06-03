namespace MapMaker.Editor.Commands;

public interface IEditorCommand
{
    string Name { get; }

    void Execute();
    void Undo();
} 