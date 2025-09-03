import { Link } from "react-router-dom";
import { Card } from "react-bootstrap";
import { Post } from "../hooks/usePosts";

interface PostsListProps {
  posts: Post[];
}

export function PostsList({ posts }: PostsListProps) {
  if (!posts.length) return <div>게시글이 없습니다.</div>;
  return (
    <div>
      {posts.map((p) => (
        <Card className="mb-3" key={p.id}>
          <Card.Body>
            <Card.Title>
              <Link to={`/posts/${p.id}`}>{p.title}</Link>
            </Card.Title>
            <div className="text-muted">{new Date(p.created).toLocaleString()}</div>
          </Card.Body>
        </Card>
      ))}
    </div>
  );
}
