import { useNavigate, Link } from "react-router-dom";
import { CreatePost } from "../components/CreatePost";
import { Button, ButtonGroup, Card } from "react-bootstrap";

export function CreatePage() {
  const navigate = useNavigate();

  return (
    <div>
      <div className="mb-3">
        <Link to="/" className="btn btn-outline-secondary">List</Link>
      </div>

      <Card>
        <Card.Body>
          <h1 className="h4 mb-3">New Post</h1>
          <CreatePost onCreated={() => navigate("/")} />
        </Card.Body>
      </Card>
    </div>
  );
}
