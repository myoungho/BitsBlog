import { BrowserRouter, Routes, Route, Link } from "react-router-dom";
import { ListPage } from "./pages/ListPage";
import { CreatePage } from "./pages/CreatePage";
import { EditPage } from "./pages/EditPage";
import { PostPage } from "./pages/PostPage";
import { Container, Navbar, Nav } from "react-bootstrap";

export default function App() {
  return (
    <BrowserRouter>
      <Navbar bg="dark" data-bs-theme="dark" expand="lg">
        <Container>
          <Navbar.Brand as={Link} to="/">BitsBlog</Navbar.Brand>
          <Navbar.Toggle aria-controls="navbar" />
          <Navbar.Collapse id="navbar">
            <Nav className="ms-auto">
              <Nav.Link as={Link} to="/">목록</Nav.Link>
              <Nav.Link as={Link} to="/new">새 글 작성</Nav.Link>
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <main>
        <Container className="my-4">
          <Routes>
            <Route path="/" element={<ListPage />} />
            <Route path="/new" element={<CreatePage />} />
            <Route path="/edit/:id" element={<EditPage />} />
            <Route path="/posts/:id" element={<PostPage />} />
          </Routes>
        </Container>
      </main>
    </BrowserRouter>
  );
}
