using NTSED.Models;
using NTSED.ProgramTypes;

namespace NTSED
{
    public class ProgramManagementService
    {
        private readonly Dictionary<int, BaseProgram> programs = new Dictionary<int, BaseProgram>();
        private int lastId = 1;
        private readonly IServiceProvider serviceProvider;

        public ProgramManagementService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        private int GenerateNewId() => lastId++;

        public void Clear()
        {
            foreach (var (_, value) in programs)
            {
                value.Dispose();
            }
            programs.Clear();
            lastId = 1;
            GC.Collect();
        }

        public int NewProgram(ProgramType type)
        {
            BaseProgram? program;
            switch (type)
            {
                case ProgramType.Computer:
                    program = new ComputerProgram(serviceProvider.GetRequiredService<ILogger<ComputerProgram>>());
                    _ = program.Initialize();
                    break;
                case ProgramType.TCom:
                    program = new TComProgram(serviceProvider.GetRequiredService<ILogger<TComProgram>>());
                    _ = program.Initialize();
                    break;
                default:
                    throw new ArgumentException("Unsupported Type");
            }
            var id = GenerateNewId();
            programs[id] = program;
            return id;
        }

        public BaseProgram GetProgram(int id)
        {
            var program = programs[id];
            if (program == null)
                throw new ArgumentException("Program not found.");
            return program;
        }

        public T GetProgram<T>(int id) where T : BaseProgram => (T)GetProgram(id);

        public void RemoveProgram(int id)
        {
            var program = GetProgram(id);
            program.Dispose();
            programs.Remove(id);
        }
    }
}
