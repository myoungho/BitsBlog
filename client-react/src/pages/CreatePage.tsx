import { useNavigate, Link } from "react-router-dom";
import { CreatePost } from "../components/CreatePost";

export function CreatePage() {
  const navigate = useNavigate();

  return (
    <div>
      <h1>글쓰기</h1>
      <div style={{ margin: "0 0 12px" }}>
        <Link to="/">목록으로</Link>
      </div>
      <CreatePost onCreated={() => navigate("/")} />
    </div>
  );
}

