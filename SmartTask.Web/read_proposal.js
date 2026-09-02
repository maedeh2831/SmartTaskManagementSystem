const JSZip = require('jszip');
const fs = require('fs');
const path = require('path');

const docxPath = path.join('..', 'پروپزال پروژه .docx');
const data = fs.readFileSync(docxPath);

JSZip.loadAsync(data).then(zip => {
  return zip.file('word/document.xml').async('string');
}).then(xml => {
  const matches = xml.match(/<w:t[^>]*>([\s\S]*?)<\/w:t>/g) || [];
  const texts = matches.map(m => m.replace(/<[^>]+>/g, ''));
  const full = texts.join('');
  console.log('=== LENGTH:', full.length, '===');
  console.log(full);
}).catch(e => console.error(e));
