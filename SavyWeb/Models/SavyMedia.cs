using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project2.Model;

namespace SavyWeb.Models
{
    [Route("api/[controller]")]
    public class ListController : ControllerBase
    {
        //Self generated ID
        //Use the multiple object to create a list of tasks
        //veggies + Tomato || Gym + Legs day 

        private static readonly List<TodoList> _todoItems = new List<TodoList>();

        public ListController()
        {
            if (!_todoItems.Any())
            {
                for (int i = 1; i <= 10; i++)
                {
                    _todoItems.Add(new TodoList
                    {
                        Id = i,
                        Title = $"Task {i}",
                        Description = $"Description for Task {i}",
                        IsCompleted = false
                    });
                }
            }
        }

        //GET api/task
        [HttpGet]
        public ActionResult<IEnumerable<TodoList>> Get()
        {
            return Ok(_todoItems);
        }
        //GET api/task/1

        [HttpGet("{id}")]
        public ActionResult<TodoList> Get(int id)
        {
            var todoItem = _todoItems.FirstOrDefault(x => x.Id == id);
            if (todoItem == null)
            {
                return NotFound();
            }
            return Ok(todoItem);
        }
        //POST api/task

        [HttpPost]
        public ActionResult Post([FromBody] List<TodoList> todoItems)
        {
            if (todoItems == null || !todoItems.Any())
            {
                return BadRequest("Invalid data.");
            }

            foreach (var todoItem in todoItems)
            {
                
                todoItem.Id = _todoItems.Count + 1;
                _todoItems.Add(todoItem);
            }

            return Ok(todoItems);
        }


        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] TodoList todoItem)
        {
            if (id != todoItem.Id)
            {
                return BadRequest();
            }
            var todoItemToUpdate = _todoItems.FirstOrDefault(x => x.Id == id);
            if (todoItemToUpdate == null)
            {
                return NotFound();
            }
            todoItemToUpdate.Title = todoItem.Title;
            todoItemToUpdate.Description = todoItem.Description;
            todoItemToUpdate.IsCompleted = todoItem.IsCompleted;

            return NoContent();
        }

        //DELETE api/task

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var todoItemToDelete = _todoItems.FirstOrDefault(x => x.Id == id);
            if (todoItemToDelete == null)
            {
                return NotFound();
            }
            _todoItems.Remove(todoItemToDelete);
            return NoContent();
        }
    }
}   