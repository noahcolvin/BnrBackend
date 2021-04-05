# Big Nerd Ranch: Coding Exercise (.NET/C#)

## Features
- Backend for a series of user's posts
- ASP.NET Core WebAPI service
- SQLite database with EF Core ORM

## Endpoints
| Endpoint | Method | Parameters | Returns |
| -------- | ------ | ---------- | ------- |
| /api/posts | GET | | List of all `Post`s |
| /api/posts?userId={id} | GET | `User`'s `Id` | List of all `Post`s by `User` |
| /api/posts/{id} | GET | `Id` of the post to retrieve | Found `Post` or 404 |
| /api/posts | POST | `Post` to add | Added `Post` or 400 if missing required fields or `Id` already exists |
| /api/posts/{id} | PUT | `Id` to update and `Post` with data | 204 if added or 400 is missing required fields or `Id` mismatches |
| /api/posts/{id} | DELETE | `Id` of the post to delete | Deleted `Post` or 404 |

## Sample Data
### Post
```
{
  "id": 1,
  "title": "Sample",
  "body": "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
  "user": {
    "id": 1,
    "name": "Some Person",
    "email": "user@email.com",
    "expertise": "Posting"
  }
}
```

## Testing
- Dockerized, deployed, and available at https://bnr-backend.herokuapp.com/api/posts
- Unit tests for `PostsController` mocking the repository
- Integration tests for `PostRepository` to test database interaction to in-memory DB
- Integration tests for controller to ensure correct route/database/system configuration
  - Host server within test class

## Not Complete/Notes
- No authentication
- Instead of directly returning/sending `userId` on a `Post`, uses the entire `User`
- Only a few fields are required on a `Post`: `Title`, `Body`, and `User`
  - Could add length or other constraints
  - Missing verification of a valid `User` on POST/PUT
- Unable to create a `User`
  - Have to add `Post`s using current `User`s
  - No constraints on `User` (not that you can make one at present)
- Disabled HTTPS redirect for ease of testing