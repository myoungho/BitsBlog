import { BrowserRouter, Routes, Route } from "react-router-dom";
import { ListPage } from "./pages/ListPage";
import { CreatePage } from "./pages/CreatePage";
import { EditPage } from "./pages/EditPage";

export default function App() {
  return (
    <BrowserRouter>
      <div style={{ maxWidth: 720, margin: "24px auto", padding: "0 12px" }}>
        <Routes>
          <Route path="/" element={<ListPage />} />
          <Route path="/new" element={<CreatePage />} />
          <Route path="/edit/:id" element={<EditPage />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}
